# Dollarshop — Microservices architecture explained

> A self-contained walkthrough of the microservices architecture: how the four services are decomposed, how they talk, how the checkout Saga works, and where every concept lives in the codebase. The companion document is [`ddd_explained.md`](ddd_explained.md), which explains the domain model these services materialize.
>
> The formal design document is [`Microservices/Microservices-Architecture.md`](Microservices/Microservices-Architecture.md); this file is the "speak it in your own words" version, with code locations.

---

## 1. The big picture in one paragraph

Dollarshop is decomposed into **four microservices** that map 1-to-1 onto the four Bounded Contexts from DDD: `identity-service`, `catalog-service`, `sales-service` (the Core), and `finance-service`. Each owns its own database (PostgreSQL, one logical DB per service), they talk to each other through two channels — REST when the caller needs an answer to continue, RabbitMQ when it only needs to notify — and the checkout flow that spans Sales/Catalog/Finance is run as a **Saga of orchestration** owned by `sales-service`. To make that Saga safe across processes, the Sales service uses a **Transactional Outbox** to publish events, an **Anti-Corruption Layer** to translate Finance's vocabulary, and **resilience patterns** (retry + circuit breaker + bulkhead) on its outbound REST calls. The whole thing boots with `docker compose up --build`.

---

## 2. Why microservices for this project (and the trade-off we accepted)

A monolith would have been simpler. We chose to split because the four contexts have **different change rates, vocabularies, and load profiles**: discount rules change in Sales every week; payment-gateway logic in Finance changes when the provider does; identity barely changes at all. Coupling them into one deploy means they all redeploy together and all scale together — paying compute for the slow contexts to support the busy ones.

The honest trade-off we acknowledged:

| What we gain | What we pay |
| :-- | :-- |
| Independent deployment per service | Operational complexity (network, multiple containers, log correlation) |
| Failure isolation (Finance can be down without breaking the catalog) | Network latency on every cross-service call |
| Per-service tech freedom | No ACID transaction across services → eventual consistency, Sagas |

The decision is justified because Dollarshop's load is concentrated in checkout. In a monolith we'd over-provision the entire system to handle a spike in one context; with microservices we scale `sales-service` independently.

**The guiding principle:** *one Bounded Context = one microservice*. No exceptions. This rule prevents the classic decomposition mistake of cutting by technical layer (one "API service", one "DB service", one "auth service") instead of by business boundary.

---

## 3. The four services — what they own and what they expose

| Service | DDD classification | DB | Host port | What it exposes |
| :-- | :-: | :-- | :-: | :-- |
| `identity-service` | Supporting | identity_db | 5001 | `POST /auth/login`, `GET /users/{id}`, `POST /customers` |
| `catalog-service` | Supporting | catalog_db | 5002 | `POST /stock-reservations`, `POST /stock-reservations/release`, `GET /products` |
| **`sales-service`** | **CORE** | sales_db | **5003** | `POST /carts`, `GET /carts/{id}`, `POST /carts/{id}/items`, `POST /carts/{id}/checkout`, … |
| `finance-service` | Generic | — | internal | RabbitMQ consumer (no HTTP exposed) |

`finance-service` deliberately has **no public HTTP surface**. It only consumes `finance.payment.request` messages and publishes `sales.payment.result`. Reducing the attack surface of the domain that handles money is a security decision.

### 3.1 Database-per-Service

Each service has **exclusive ownership** of its database. No service reads another's tables; the only way to get data out is the public API or the events the owning service publishes.

Where this lives:
- The PostgreSQL container hosts three logical databases — `sales_db`, `catalog_db`, `identity_db`.
- `infra/postgres/init/01-create-databases.sql` creates `catalog_db` and `identity_db` on the **first** boot of the container (`sales_db` is created via `$POSTGRES_DB` in compose).
- Each service has its own EF Core `DbContext` and its own `InitialCreate` migration.

**Why this matters:** if catalog had a lock-contention issue tomorrow, it wouldn't affect Finance. If we wanted to swap Catalog to MongoDB, no other service would need to change. The boundary is enforced at the infrastructure level, not by convention.

### 3.2 Each service follows the same 4-layer template

Sales, Catalog, and Identity all use the same internal layout (Finance is a single-project stub):

```
<Service>.Domain/         ← Aggregates, VOs, domain events. ZERO dependencies.
<Service>.Application/    ← CQRS commands/queries via MediatR. References Domain only.
<Service>.Infrastructure/ ← EF Core, ORM mappings, repositories, messaging. References Domain + Application.
<Service>.Api/            ← ASP.NET Core: thin controllers + HTTP DTOs. References Application + Infrastructure.
```

**Dependencies always point inward.** The `Domain` `.csproj` literally has no `<PackageReference>` or `<ProjectReference>`. The compiler enforces this — you cannot accidentally pull EF Core into a domain class.

---

## 4. How services communicate — two channels, one rule

> Synchronous when the caller needs the answer to continue. Asynchronous when it just needs to notify.

### 4.1 Synchronous: REST over HTTP (sales → catalog)

When `sales-service` checks out a cart, it must reserve stock and **know whether the reservation succeeded** before it can proceed. That requires a synchronous answer.

In the code:
- `src/sales-service/Sales.Application/Abstractions/IStockReservationService.cs` — the **port** (interface) the Application layer declares.
- `src/sales-service/Sales.Infrastructure/Rest/CatalogStockClient.cs` — the **adapter** that implements that port using a typed `HttpClient`.
- `src/catalog-service/Catalog.Api/Controllers/StockReservationsController.cs` — Catalog's REST endpoint that answers it.

The body shape is mirrored on both sides (`{ cartId, lines: [{ productId, quantity }] }`) — they're separate copies in each service's own folder. **No shared assembly.** That's deliberate: if we shared a `Contracts.dll` between services, we'd recreate the monolith's coupling through the back door.

**The trade-off** of synchronous calls is **temporal coupling**: if `catalog-service` is down, `sales-service`'s checkout call fails. The resilience patterns (§6) are how we cap the damage.

### 4.2 Asynchronous: RabbitMQ topic exchange (sales ↔ finance)

When `sales-service` needs `finance-service` to process a payment, the answer comes back in seconds, not in the HTTP timeout window — and the cart shouldn't sit on the wire waiting. We publish a message and the consumer reacts to it.

**Topology** (one shared exchange, two queues):

```
sales-service ──► [ exchange: dollarshop.events (topic, durable) ]
                       │
                       │  routing key: finance.payment.request
                       ▼
                  [ queue: finance.payment-requests ]
                       │
                       ▼
                  finance-service consumer
                       │
                       │  (waits 2s in the demo stub, then publishes:)
                       ▼
                  routing key: sales.payment.result
                       │
                       ▼
[ exchange: dollarshop.events ]
                       │
                       ▼
                  [ queue: sales.payment-results ]
                       │
                       ▼
                  sales-service consumer
```

In the code:
- `src/sales-service/Sales.Infrastructure/Messaging/RabbitMqEventPublisher.cs` — declares the exchange, publishes with `Persistent = true`.
- `src/sales-service/Sales.Infrastructure/Messaging/PaymentResultConsumer.cs` — the inbound `BackgroundService` that consumes results, ACL-translates them, and dispatches the resulting integration event via MediatR.
- `src/finance-service/PaymentProcessor.cs` — the corresponding consumer/publisher on the Finance side.

**Why a topic exchange and not a direct/fanout?** Topics let each consumer subscribe with a routing-key pattern (`finance.*`, `sales.payment.*`) without the publisher having to know which services exist. Adding a marketing service that listens for `sales.carrito.abandonado` later means binding a new queue with that routing key — `sales-service` doesn't change at all.

---

## 5. The checkout Saga — the most important flow in the system

This is the moment where DDD meets distributed systems. Carts live in `sales_db`, payments would live in `finance_db` — there is no ACID transaction across both. Closing a sale is a **distributed transaction**, and we solve it with the **Saga pattern**.

### 5.1 Orchestration, not choreography

A choreography Saga has each service reacting to events on its own; an orchestration Saga has one service explicitly conducting the flow. We chose **orchestration** because checkout is the Core process — its logic should be **centralized and auditable**, not scattered across services where no one "owns" it. The trade-off (one coordination point) is worth the clarity.

The orchestrator: `src/sales-service/Sales.Application/Sagas/CheckoutSagaOrchestrator.cs`. It's the physical materialization of the DDD `CheckoutApplicationService` — the same concept, now living in a service that talks to its collaborators over the network.

### 5.2 The happy path — step by step

```
                                                                    sales_db
                                                              ┌──────────────┐
                                                              │ carts        │
                                                              │ outbox_msgs  │
                                                              └──────────────┘

  client                sales-service                 catalog-service          RabbitMQ           finance-service
    │                        │                              │                     │                     │
    │── POST /checkout ─────►│                              │                     │                     │
    │                        │── POST /stock-reservations ─►│                     │                     │
    │                        │◄────── 200 OK ───────────────│                     │                     │
    │                        │                                                                          │
    │                        │   cart.Checkout()                                                        │
    │                        │   → records CheckoutIniciado domain event                                │
    │                        │   → orchestrator builds OrderPlaced                                      │
    │                        │   → ACL maps to PaymentRequestMessage                                    │
    │                        │   → all of this committed to sales_db                                    │
    │                        │     IN ONE TRANSACTION (Outbox row included)                             │
    │◄── 202 Accepted ───────│                                                                          │
    │                                                                                                   │
    │                  (background OutboxRelay)                                                         │
    │                        │── publish to dollarshop.events ──────────────►│                          │
    │                        │   routing key: finance.payment.request        │                          │
    │                        │                                               │── deliver ──────────────►│
    │                        │                                                                  ┌───────┤
    │                        │                                                                  │ wait  │
    │                        │                                                                  │  2s   │
    │                        │                                                                  └───────┤
    │                        │                                               │◄── publish ──────────────│
    │                        │                                               │ routing key: sales.payment.result
    │                        │◄── deliver to PaymentResultConsumer ──────────│  outcome: AUTHORIZED
    │                        │                                                                          │
    │                        │   ACL translates → PagoAprobado (MediatR)                                │
    │                        │   PagoAprobadoHandler.Handle()                                           │
    │                        │   → cart.ConfirmSale()                                                   │
    │                        │   → records VentaCerrada                                                 │
    │                        │   → COMMIT (cart.Status = "Closed")                                      │
    │                        │                                                                          │
    │── GET /carts/{id} ────►│                                                                          │
    │◄── { status: "Closed" }│
```

The whole thing takes ~2 seconds end-to-end (the artificial delay in `finance-service`).

### 5.3 The compensation path — when payment is rejected

Compensations run in **reverse order** of the steps that succeeded. They are idempotent.

| Step | Forward action | Compensation |
| :-- | :-- | :-- |
| 1 | Reserve stock in `catalog-service` | Release stock |
| 2 | Cart transitions to `CheckedOut` | `cart.RevertCheckout()` → status `Reverted` |
| 3 | Confirm sale in `sales-service` | (skipped if rejected) |

When `PagoRechazado` arrives, `src/sales-service/Sales.Application/IntegrationEvents/PagoRechazadoHandler.cs` runs the compensation: it loads the cart, calls `RevertCheckout()` (which records `CheckoutRevertido`), then calls `_stock.ReleaseAsync()` to undo the reservation in Catalog.

**Why orchestration makes the compensation safe:** the orchestrator knows which steps completed, so it knows which compensations to run. In a choreography Saga, every service would need to know about every other service's failure modes — and a partial failure becomes very hard to reason about.

### 5.4 Idempotency — required because delivery is at-least-once

The handlers (`PagoAprobadoHandler`, `PagoRechazadoHandler`) start with:

```csharp
if (cart.Status != CartStatus.CheckedOut)
{
    return;  // already processed — duplicate delivery is a safe no-op
}
```

That's the idempotency guard. Because RabbitMQ may deliver a message more than once (broker restart, network blip), a second `PagoAprobado` for the same cart must not double-confirm the sale.

---

## 6. Transactional Outbox — why events reach the broker if and only if the business change committed

### 6.1 The dual-write problem

Naively, `sales-service` would do this on checkout:

```
1. UPDATE carts SET status = 'CheckedOut'       (DB transaction)
2. rabbitmq.publish(PaymentRequestMessage)      (network call)
```

If the process crashes between (1) and (2), the cart is checked out but Finance never hears about it — sale lost. If it crashes between (2) and the user response, the message is sent but the DB change rolled back — payment requested with no cart to attach to. Either way the system is inconsistent.

### 6.2 The Outbox solution

The event isn't published directly. It's inserted into an `outbox_messages` table **inside the same DB transaction** as the business change:

```
BEGIN
  UPDATE carts SET status = 'CheckedOut' …
  INSERT INTO outbox_messages (…)
COMMIT
```

Either both rows appear or neither does. Then a separate process — the **Outbox Relay** — polls the table, publishes pending messages to RabbitMQ, and marks them processed.

**The event reaches the broker if and only if the business transaction committed.** That's the guarantee.

### 6.3 Where it lives in the code

| File | Role |
| :-- | :-- |
| `Sales.Infrastructure/Persistence/OutboxMessage.cs` | The outbox row entity |
| `Sales.Infrastructure/Persistence/SalesDbContext.cs` | `CommitAsync()` drains `aggregate.DomainEvents` into outbox rows **before** `SaveChangesAsync` — same transaction |
| `Sales.Infrastructure/Outbox/OutboxRelay.cs` | `BackgroundService` that polls every few seconds and publishes |
| `Sales.Infrastructure/Outbox/OutboxRouting.cs` | Maps event type → routing key |

The Outbox is also where the ACL payment request gets queued. `FinancePaymentGateway.RequestPaymentAsync` doesn't publish anything itself — it enqueues an `OutboxMessage` and the relay handles delivery.

### 6.4 The cost — delivery is at-least-once, not exactly-once

The relay can crash after publishing but before marking the row processed. On restart, it'll publish the same message again. That's why **every consumer is idempotent** — duplicates produce the same result as singles (§5.4).

You can inspect the Outbox live during the demo:

```sql
SELECT message_type, processed_on IS NOT NULL AS sent, retry_count
FROM outbox_messages
ORDER BY occurred_on;
```

This is the kind of visibility the Outbox is designed to give — every event the system ever wanted to publish is visible in the DB.

---

## 7. The Anti-Corruption Layer — making Sales survive Finance's vocabulary

> Already detailed in [`ddd_explained.md`](ddd_explained.md) §7. This section explains its role in the distributed picture.

In a distributed system, the ACL becomes even more important than it was in DDD: each service speaks its own dialect over the wire. If Sales received Finance messages directly and started referencing types like `PaymentResultMessage` in its handlers, every change to Finance's API (which is glued to whatever payment provider Finance uses) would cascade into Sales.

The ACL **stops the cascade at the Sales infrastructure boundary**:

```
[outside the ACL]                         [inside the ACL]                                [domain code]
RabbitMQ message ───► PaymentResultConsumer ───► FinanceAclMapper ──► PagoAprobado ──► PagoAprobadoHandler
(JSON, Finance shape)   (deserializes Finance contract)   (translates)   (Sales integration event)   (calls cart.ConfirmSale())
```

Finance's `PaymentRequestMessage` / `PaymentResultMessage` types live only in `Sales.Infrastructure/Acl/Contracts/`. Nothing outside `Sales.Infrastructure` references them. You can grep:

```bash
grep -r "PaymentRequestMessage" src/sales-service/Sales.Domain        # → nothing
grep -r "PaymentRequestMessage" src/sales-service/Sales.Application   # → nothing
```

That's the wall.

---

## 8. Resilience — surviving sync calls in an unreliable network

REST introduces **temporal coupling**: when `sales-service` calls `catalog-service`, both must be up. We mitigate that with three classic patterns, applied as a single ASP.NET Core *standard resilience handler* on the typed `HttpClient`:

| Pattern | Risk it mitigates |
| :-- | :-- |
| **Retry with exponential backoff** | Transient failures (one-off network blip, GC pause) — try again with growing waits so we don't hammer a recovering service |
| **Circuit breaker** | Cascading failure — if calls keep failing, "open" the breaker and fail fast instead of exhausting threads on hopeless calls |
| **Bulkhead** (concurrency limiter) | Resource exhaustion — a separate connection pool per dependency means a slow Catalog can't starve the calls Sales needs to make to Identity |

In the code: `src/sales-service/Sales.Infrastructure/DependencyInjection.cs`

```csharp
services
    .AddHttpClient<IStockReservationService, CatalogStockClient>(…)
    .AddStandardResilienceHandler(options =>
    {
        options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(5);
    });
```

That single line bundles retry + circuit breaker + timeout + concurrency limiter (Polly v8 under the hood). The catalog client itself contains no resilience code — concerns separated.

**The combined behavior:** retry handles the transient, breaker handles the persistent, bulkhead guarantees that even when one dependency is misbehaving, the others remain reachable.

---

## 9. API Gateway and exposed surface

Today the demo calls `sales-service` directly at `:5003`. In production all client traffic should enter through a single **API Gateway** that handles cross-cutting concerns (TLS termination, edge authentication, rate limiting, request routing). The current `docker-compose.yml` does not include a gateway — that's deliberate scope-cutting for the academic deliverable, and the architecture document marks it as the natural next step.

`finance-service` is intentionally *never* exposed at the edge (`§4.1` of the architecture doc). It's reachable only from inside the cluster, only via RabbitMQ. This is a security decision for the domain that handles money — reducing its attack surface to zero externally.

---

## 10. Observability — how you watch it run

Three windows give you the full picture during a demo:

| Window | What it shows | Command / URL |
| :-- | :-- | :-- |
| Service logs | The Saga unfolding step by step | `docker compose logs -f sales-service catalog-service finance-service` |
| RabbitMQ management UI | Exchange topology, queue depth, message rate | `http://localhost:15672` (guest / guest) |
| PostgreSQL psql | Cart status, Outbox progression, seeded data | `docker compose exec postgres psql -U sales -d sales_db` |

The Outbox is your best friend for explaining the system: it's a literal table of "every business fact this service wanted the world to know about", with timestamps and a `processed_on` column showing when each one reached the broker.

---

## 11. Deployment — `docker compose up --build`

The runnable demo is just a `docker-compose.yml` at the repo root. It defines nine logical units:

| Compose service | What it is |
| :-- | :-- |
| `postgres` | Shared PostgreSQL host (one container, three logical DBs created by `infra/postgres/init/`) |
| `rabbitmq` | Broker with management UI |
| `sales-migrator`, `catalog-migrator`, `identity-migrator` | One-shot containers that emit idempotent SQL from EF migrations and apply it before the API starts |
| `sales-service`, `catalog-service`, `identity-service`, `finance-service` | The four microservices |

Boot ordering is enforced by compose `depends_on` with healthchecks:

```
postgres (healthy) → migrators (completed) → API services
rabbitmq (healthy) → sales-service + finance-service
```

That guarantees the database schema exists before any service tries to use it, and the broker is reachable before consumers start. No retry loops in startup code.

---

## 12. What the folder structure tells you about the design

```
Dollarshop/
├── docker-compose.yml          ← the runnable system: who depends on whom
├── infra/postgres/init/        ← first-boot SQL: creates catalog_db + identity_db
├── Dollarshop.Microservices.slnx
├── src/
│   ├── sales-service/          ← CORE — four-layer DDD shape
│   │   ├── Sales.Domain/
│   │   ├── Sales.Application/
│   │   ├── Sales.Infrastructure/
│   │   │   ├── Persistence/    ← EF Core, repositories, Outbox table
│   │   │   ├── Outbox/         ← OutboxRelay BackgroundService
│   │   │   ├── Messaging/      ← RabbitMQ publisher + consumer
│   │   │   ├── Acl/            ← Anti-Corruption Layer (Finance translator)
│   │   │   ├── Rest/           ← Resilient HTTP client to Catalog
│   │   │   └── Sales.Migrator.Dockerfile   ← one-shot DB migrator
│   │   └── Sales.Api/
│   ├── catalog-service/        ← same four-layer shape
│   ├── identity-service/       ← same four-layer shape
│   └── finance-service/        ← single-project consumer stub
└── docs/
    ├── DDD/                    ← Entrega 3 — DDD design (source of truth)
    ├── Microservices/          ← Entrega 3 — Microservices architecture (source of truth)
    ├── ddd_explained.md        ← companion to this file
    └── microservices_explained.md
```

The structure reads top-down: `docker-compose.yml` tells you what runs, `src/` tells you each service is its own bounded context, the four-layer split inside each service tells you dependencies always point inward. **The structure is the design.**

---

## 13. How to defend the most likely questions

**"Why one Bounded Context per microservice?"**
Because cutting any other way (by technical layer, by entity, by team) re-creates the coupling we wanted to eliminate. A BC is, by definition, a boundary inside which one model is consistent and outside which the model changes. That's the right unit of deployment too.

**"Why orchestration Saga and not choreography?"**
Checkout is the Core process. We want its logic centralized and auditable — one place to look when something goes wrong. Choreography would scatter that logic across all participants. The trade-off (one coordination point in `sales-service`) is worth the clarity for the most valuable process in the system.

**"Why Database-per-Service?"**
To make the boundaries real. If services shared a DB, schema changes would propagate through the back door, defeating the autonomy we paid for. The cost — no JOINs across services — is mitigated by API composition (gateway aggregates) and event-based replication (the cart stores a *snapshot* of the product, not a live reference).

**"What does the Outbox give you?"**
Atomicity between the business change and the event publish. Without it, dual-write means we can lose events on crash. With it, either both rows commit (business + outbox) or neither do, and the relay handles publishing — at-least-once, which is why all consumers are idempotent.

**"Why publish events to a topic exchange instead of direct queues?"**
Decoupling. The publisher doesn't need to know who's listening — it tags the message with a routing key and the broker matches subscribers' bindings. Adding a new consumer tomorrow (e.g. analytics on `sales.carrito.abandonado`) means binding a queue, not changing publisher code.

**"What happens if `finance-service` is down when a checkout fires?"**
The PaymentRequest sits in the Outbox table waiting to be relayed. When the broker accepts it, the message sits in `finance.payment-requests` queue waiting to be consumed. When `finance-service` boots, it processes the backlog. No checkouts are lost; the user just waits longer for `PagoAprobado`. (The 202 Accepted response is what lets the system absorb this without making the user wait.)

**"What happens if `catalog-service` is down?"**
The synchronous REST call from sales→catalog fails. After retries the circuit breaker opens, the call returns fast, and the checkout returns 5xx to the user. The cart stays `Active` (no domain transition happened), so the user can retry later. The bulkhead means even if Catalog is down, calls to Identity still work.

**"How would you scale this?"**
`sales-service` can run as multiple replicas behind a load balancer; the only shared state is `sales_db` (which handles concurrent writes via its row-level locking). The Outbox would need an extra `FOR UPDATE SKIP LOCKED` guard if multiple relays run in parallel, but the table-based approach is otherwise horizontally safe. Identity, Catalog, and Finance scale the same way.

**"Where would you put an API Gateway?"**
In front of `sales`, `catalog`, and `identity` at port 80/443. Finance stays internal. The gateway terminates TLS, validates JWTs against Identity once, applies rate limits, and routes to internal services by URL prefix. YARP (Microsoft's reverse proxy) would be the natural choice in .NET.

**"How do you correlate logs across services?"**
Today by hand (timestamps + cart GUID). The next polish step is a `correlation-id` header generated at the edge and propagated through HTTP calls and message headers, then logged via Serilog enrichers. The architecture doc lists this in §7.3 as the observability next step.

---

## 14. What to demo, in this order

When the professor is watching, this is the ~3-minute path that exercises everything:

1. **Show the stack starting**: `docker compose up --build`, wait for "Seeded 3 demo products" + "Seeded demo customer".
2. **Show the topology**: open RabbitMQ UI at `http://localhost:15672`. Point at the exchange `dollarshop.events`, the queues `finance.payment-requests` and `sales.payment-results`, the bindings on them.
3. **Run the demo flow** (the curl commands from `docs/DEMO.md` or the README): create cart → add 2 items → checkout → wait 3s → cart status `"Closed"`.
4. **Open the Outbox**: `psql` into `sales_db`, `SELECT message_type, processed_on IS NOT NULL FROM outbox_messages ORDER BY occurred_on;` — you'll see the events in order, all marked sent.
5. **Tail the logs**: `docker compose logs -f sales-service finance-service` — point at the timestamps to show the 2-second gap is `finance-service`'s simulated processing.
6. **Show the ACL in code**: open `Sales.Infrastructure/Acl/FinanceAclMapper.cs` — 40 lines, both directions visible at once.
7. **Show the orchestrator**: open `Sales.Application/Sagas/CheckoutSagaOrchestrator.cs` — 80 lines, all the steps from the sequence diagram are there as code.
8. **Show the rejection rehearsal** (optional, if you have time): flip `Outcome: "AUTHORIZED"` → `"DECLINED"` in `finance-service`, rebuild, run again — cart status becomes `"Reverted"`, catalog logs show the stock release. That's the compensation path.

That sequence touches every box on the architecture rubric: decomposition (§3), communication (§4), Saga (§5), Outbox (§6), ACL (§7), resilience (§8), gateway/exposure (§9), deployment (§11), observability (§10), and DDD layering (§3.2 + the companion document).
