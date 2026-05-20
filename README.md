# Dollarshop

A microservices-based e-commerce backend built in C#/.NET 10. The system models the shopping/checkout flow as four independently deployable services communicating over REST and RabbitMQ, with the checkout orchestrated as a distributed Saga.

## Architecture overview

Four Bounded Contexts → four microservices, each owning its own database (Database-per-Service, §2.3). `sales-service` is the **Core** and the only orchestrator; `finance-service` is internal — clients reach it only through the Saga.

```
                       ┌────────────────────┐
                       │  client / curl     │
                       └─────────┬──────────┘
        ┌──────────────┬─────────┼─────────────────┐
        │              │         │                 │
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

---

| Service | Port | Role | DB | Layers |
| :-- | :-: | :-- | :-: | :-- |
| `sales-service` | 5003 | Core. Cart aggregate, checkout Saga, Outbox, ACL to Finance | sales_db | Domain · Application · Infrastructure · Api |
| `catalog-service` | 5002 | Stock reservations, product catalog | catalog_db | Domain · Application · Infrastructure · Api |
| `identity-service` | 5001 | Customer profiles, authentication | identity_db | Domain · Application · Infrastructure · Api |
| `finance-service` | internal | Auto-approves payments after 2 s (demo stub) | — | single-project worker |

### Checkout Saga

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
---

## Quick start 🚀

### Prerequisites
* Docker with Compose v2 (`docker compose version` → v2+)
* `curl` (or HTTPie); `jq` recommended for pretty JSON

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

### Stop the stack

```bash
docker compose down          # keep databases
docker compose down -v       # also wipe postgres-data (re-seeds on next up)
```
--- 

## Tech stack

- **.NET 10** · ASP.NET Core controllers · MediatR (CQRS) · FluentValidation
- **EF Core 10** + **Npgsql** (PostgreSQL provider)
- **RabbitMQ.Client v7** (native, async API)
- **Microsoft.Extensions.Http.Resilience** (Polly v8 — retry, circuit breaker, bulkhead)
- **Serilog** (structured logs, request logging)
- **Swashbuckle** (OpenAPI / Swagger UI)
- **Docker Compose** for local orchestration
