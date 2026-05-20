# Dollarshop

A microservices-based e-commerce backend built in C#/.NET 10, designed for the Software Architecture course at Universidad Pontificia Bolivariana (Entrega 3).

The system models the shopping/checkout flow as four independently deployable services communicating over REST and RabbitMQ, with the checkout orchestrated as a distributed Saga. The full design lives in [`docs/Microservices/Microservices-Architecture.md`](docs/Microservices/Microservices-Architecture.md) and [`docs/DDD/Entrega 3 - DDD.md`](docs/DDD/Entrega 3 - DDD.md).

## Architecture overview

Four Bounded Contexts → four microservices, each owning its own database (Database-per-Service, §2.3). `sales-service` is the **Core** and the only orchestrator; `finance-service` is internal — clients reach it only through the Saga.

```
                       ┌────────────────────┐
                       │  client / curl     │
                       └─────────┬──────────┘
        ┌──────────────┬─────────┼──────────┬────────────────┐
        │              │         │          │                │
   ┌────▼─────┐  ┌─────▼────┐  ┌─▼────────┐  ┌──────────┐
   │identity- │  │ catalog- │  │  sales-  │  │ finance- │
   │ service  │  │ service  │  │ service  │  │ service  │
   │  :5001   │  │  :5002   │  │  :5003   │  │ internal │
   └────┬─────┘  └────┬─────┘  └─┬────────┘  └────┬─────┘
        │             │ REST     │  ┌────────────┐│
        │             └──────────┤  │ RabbitMQ   ││
        │                        ├──┤ dollarshop.├┘
        │                        │  │ events     │
        │  PostgreSQL :5431      │  └────────────┘
        ▼  identity_db ┐         ▼  topic exchange
                       ├── catalog_db
                       └── sales_db
```

| Service | Port | Role | DB | Layers |
| :-- | :-: | :-- | :-: | :-- |
| `sales-service` | 5003 | Core. Cart aggregate, checkout Saga, Outbox, ACL to Finance | sales_db | Domain · Application · Infrastructure · Api |
| `catalog-service` | 5002 | Stock reservations, product catalog | catalog_db | Domain · Application · Infrastructure · Api |
| `identity-service` | 5001 | Customer profiles, authentication | identity_db | Domain · Application · Infrastructure · Api |
| `finance-service` | internal | Auto-approves payments after 2 s (demo stub) | — | single-project worker |

### Checkout Saga (the interesting part)

`POST /carts/{id}/checkout` kicks off the orchestrated Saga (`docs/Microservices/Microservices-Architecture.md §3.3`):

```
sales-service     catalog-service    RabbitMQ      finance-service
     │                  │              │                │
     │── POST stock ───►│              │                │
     │◄── 200 OK ───────│              │                │
     │                  │              │                │
     │── publish CheckoutIniciado ────►│                │
     │   (via Outbox → relay → broker)─┼── deliver ────►│
     │                                 │                │ wait 2s
     │                                 │◄── PagoAprobado┤
     │◄────── consume + ACL translate ─┤                │
     │                                                  │
     │  cart.ConfirmSale() → status "Closed"
     │
     │  (or on rejection: cart.RevertCheckout() + catalog stock release)
```

Key mechanisms:
- **Transactional Outbox** (§3.4) — domain events are written into the same DB transaction as the business change; a background relay publishes them to RabbitMQ. No dual-write, at-least-once delivery.
- **Anti-Corruption Layer** (§5) — `Sales.Infrastructure/Acl/` translates Finance's vocabulary (`PaymentRequest`/`PaymentResult`) into Sales-domain integration events.
- **Resilient REST** (§3.5) — sales→catalog calls use `AddStandardResilienceHandler` (retry, circuit breaker, timeout, bulkhead).
- **Caching** — `CachedCartRepository` decorates the EF repo with `IMemoryCache`, invalidated on every write.
- **Idempotent handlers** — both integration-event handlers no-op if the cart is already in the target state, so duplicate broker delivery is safe.

## Repository layout

```
Dollarshop/
├── src/
│   ├── sales-service/
│   │   ├── Sales.Domain/           — aggregate, VOs, domain events (zero deps)
│   │   ├── Sales.Application/      — CQRS commands/queries, Saga orchestrator
│   │   ├── Sales.Infrastructure/   — EF Core, Outbox, RabbitMQ, ACL, REST client
│   │   └── Sales.Api/              — thin controllers, HTTP contracts
│   ├── catalog-service/            — same 4-layer shape
│   ├── identity-service/           — same 4-layer shape
│   └── finance-service/            — single-project consumer stub
├── infra/postgres/init/            — first-boot SQL: creates catalog_db + identity_db
├── docker-compose.yml              — full stack: 4 services + 3 migrators + Postgres + RabbitMQ
├── Dollarshop.Microservices.slnx   — solution file (13 projects)
└── docs/
    ├── Microservices/Microservices-Architecture.md   — source of truth
    └── DDD/Entrega 3 - DDD.md                        — domain model
```

## Running the project

### Prerequisites

- Docker with Compose v2 (`docker compose version` → v2+)
- `curl` (or HTTPie); `jq` recommended for pretty JSON

You do **not** need .NET, PostgreSQL, or RabbitMQ installed on the host — everything runs in containers.

### Start the stack

From the repo root:

```bash
docker compose up --build
```

First run takes 3–5 minutes (image pulls + NuGet restore + 7 builds). After that it's seconds.

Wait until you see, in order:
1. `dollarshop-postgres … database system is ready`
2. `dollarshop-rabbitmq … started TCP listener`
3. The three `*-migrator` containers print SQL then exit 0
4. The four services log `Now listening on: http://[::]:8080`
5. `dollarshop-catalog` logs `Seeded 3 demo products`
6. `dollarshop-identity` logs `Seeded demo customer`

### Verify it's up

```bash
curl http://localhost:5001/health    # identity → Healthy
curl http://localhost:5002/health    # catalog  → Healthy
curl http://localhost:5003/health    # sales    → Healthy
```

Swagger UIs: `http://localhost:5001/swagger`, `:5002/swagger`, `:5003/swagger`.

RabbitMQ management UI: `http://localhost:15672` (guest / guest).

### Run the end-to-end checkout demo

Seeded demo data you'll need to reference:

| What | GUID |
| :-- | :-- |
| Demo customer | `22222222-2222-2222-2222-222222222201` |
| Wireless Mouse · $12.99 | `11111111-1111-1111-1111-111111111101` |
| USB-C Cable · $8.50 | `11111111-1111-1111-1111-111111111102` |
| Notebook A5 · $3.99 | `11111111-1111-1111-1111-111111111103` |

Use these GUIDs exactly — random product IDs are rejected with `404 ProductNotFoundException`.

```bash
# 1. Create a cart
CART=$(curl -s -X POST http://localhost:5003/carts \
  -H 'content-type: application/json' \
  -d '{ "customerId": "22222222-2222-2222-2222-222222222201" }' \
  | jq -r .cartId)
echo "Cart: $CART"

# 2. Add items
curl -s -X POST "http://localhost:5003/carts/$CART/items" \
  -H 'content-type: application/json' \
  -d '{
    "productId":   "11111111-1111-1111-1111-111111111101",
    "productName": "Wireless Mouse",
    "unitPrice":   12.99,
    "currency":    "USD",
    "quantity":    2
  }'

# 3. Inspect the cart (status: "Active", subtotal $25.98)
curl -s "http://localhost:5003/carts/$CART" | jq

# 4. Checkout — returns 202 Accepted, the Saga continues asynchronously
curl -i -X POST "http://localhost:5003/carts/$CART/checkout" \
  -H 'content-type: application/json' \
  -d '{ "paymentMethod": "card" }'

# 5. After ~2s the cart status flips to "Closed"
sleep 3
curl -s "http://localhost:5003/carts/$CART" | jq .status   # → "Closed"
```

### Observe what happened

```bash
# Follow the Saga across services
docker compose logs -f sales-service catalog-service finance-service

# Inspect cart state and the Outbox
docker compose exec postgres psql -U sales -d sales_db -c \
  "select status, customer_id from carts;"

docker compose exec postgres psql -U sales -d sales_db -c \
  "select message_type, processed_on is not null as sent
     from outbox_messages order by occurred_on;"

# Inspect seeded products
docker compose exec postgres psql -U sales -d catalog_db -c \
  "select sku, name, price_amount, stock_level from products;"
```

In the RabbitMQ UI (`:15672`) you'll see exchange `dollarshop.events` (topic, durable) bound to queues `finance.payment-requests` and `sales.payment-results`.

### Stop the stack

```bash
docker compose down          # keep databases
docker compose down -v       # also wipe postgres-data (re-seeds on next up)
```

### Optional: rehearse the compensation path

The `finance-service` stub always approves. To see the compensating Saga (cart reverts, stock released):

In `src/finance-service/PaymentProcessor.cs`, change `Outcome: "AUTHORIZED"` to `Outcome: "DECLINED"`, then:

```bash
docker compose up -d --build finance-service
# run the demo flow again — cart status will become "Reverted"
```

## Tech stack

- **.NET 10** · ASP.NET Core controllers · MediatR (CQRS) · FluentValidation
- **EF Core 10** + **Npgsql** (PostgreSQL provider)
- **RabbitMQ.Client v7** (native, async API)
- **Microsoft.Extensions.Http.Resilience** (Polly v8 — retry, circuit breaker, bulkhead)
- **Serilog** (structured logs, request logging)
- **Swashbuckle** (OpenAPI / Swagger UI)
- **Docker Compose** for local orchestration
