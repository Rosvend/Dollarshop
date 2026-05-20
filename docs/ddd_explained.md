# Dollarshop — DDD design explained

> A self-contained walkthrough of the Domain-Driven Design decisions behind Dollarshop, mapped to the actual code so you can defend any of them in front of the professor. The companion document is [`microservices_explained.md`](microservices_explained.md), which covers how these contexts are deployed as services.
>
> The formal design document is [`DDD/Entrega 3 - DDD.md`](DDD/Entrega%203%20-%20DDD.md); this file is the "speak it in your own words" version, with code locations.

---

## 1. The big picture in one paragraph

Dollarshop is modelled around four **Bounded Contexts** that match real business clusters: *Identity & Access*, *Catalog*, *Sales / Cart*, and *Finance & Payments*. **Sales is the Core domain** because it owns what makes Dollarshop different from any other shop — composable discount policies and a checkout that produces a closed sale. Everything else exists to support that. We classify each domain (Core / Supporting / Generic) so the architectural effort is proportional to the business value, and we draw hard boundaries between contexts so a change in one (e.g. a new discount rule) doesn't ripple into the others (e.g. password hashing).

---

## 2. Why DDD at all — the problem we're solving

The previous version of this project (legacy monolith) had one big `Domain/` folder with everything mixed together: products called cart methods, invoices read product prices directly, validators lived next to payment classes. The professor's feedback was literally: *"under domain/ all the business is mixed together"*.

DDD gives us three tools that solve exactly that:

1. **Ubiquitous Language** — every concept in the code must be named like the business names it. No `OrderProcessor` if the business says *cart*; no `Helper` ever. (Glossary in §5 below.)
2. **Bounded Contexts** — instead of one giant model, several smaller models, each with its own internal vocabulary and consistency rules. A `User` inside Identity is **not** the same `User` Sales sees (Sales sees `CustomerId` — an opaque reference).
3. **Tactical patterns inside each context** — Aggregates, Entities, Value Objects, Domain Events, Domain Services. Each has a precise rule about when to use it; together they keep the business logic out of the infrastructure.

---

## 3. Strategic design — four contexts, one Core

### 3.1 The four contexts and why they were cut where they were

| Bounded Context | Classification | Business responsibility |
| :-- | :-: | :-- |
| **Identity & Access** | Supporting | Who is operating: registration, authentication, contact data |
| **Catalog** | Supporting | What we sell: products, list prices, stock |
| **Sales / Cart** | **CORE** | How the negotiation happens: cart, discounts, checkout |
| **Finance & Payments** | Generic | How we get paid and formalise it: payments, invoices |

**The cut criterion is functional cohesion, not technical similarity.** Two pieces of logic belong in the same context if they tend to change together when a business rule changes. A new discount rule should only touch *Sales*; a new payment provider should only touch *Finance*. If a change crosses contexts, you've found a bad seam.

**Classification matters because it dictates investment:**
- **Core** (Sales) — gets the deepest modeling: aggregate, VOs, domain events, an Anti-Corruption Layer to protect it. This is where we differentiate.
- **Supporting** (Identity, Catalog) — useful but not differentiating. Modeled cleanly, but no over-engineering.
- **Generic** (Finance) — could be outsourced one day (Stripe, electronic-invoicing SaaS). The ACL keeps the rest of the system independent of *whatever payment provider we end up using*.

### 3.2 Context Map — how the four talk

```
Identity ──(OHS / Conformist: CustomerId)──►   Sales (CORE)  ──(Partnership + ACL)──► Finance
Catalog  ──(Customer/Supplier: ProductRef)──►        │                                  │
                                                     └──── publishes domain events ─────┘
                                                                  via RabbitMQ
```

| Relationship | DDD pattern | What it means in code |
| :-- | :-- | :-- |
| Identity → Sales | **Open Host Service / Conformist** | Identity publishes a stable REST API (`GET /users/{id}`); Sales adapts to that shape without trying to reshape it. |
| Catalog → Sales | **Customer/Supplier** | Sales is a formal client of Catalog: negotiates the contract (`POST /stock-reservations` body shape) and Catalog commits to supporting it. |
| Sales → Finance | **Partnership + Anti-Corruption Layer** | The two teams coordinate the checkout, but Finance's vocabulary (`PaymentRequest`/`PaymentResult`) is never allowed into Sales' domain code. A mapper at the Sales-Infrastructure boundary translates both directions. |

The ACL is the single most important strategic decision in this project — that's why it gets its own section below (§7) and its own dedicated folder in code (`src/sales-service/Sales.Infrastructure/Acl/`).

---

## 4. Tactical patterns — the rules we follow inside every context

Each context uses the same vocabulary internally. Knowing these five rules is enough to read any part of the codebase.

### 4.1 Aggregate Root

A cluster of related objects that must be treated as **one unit of consistency**. The **root** is the only object outside code can hold a reference to; everything inside is reached through it.

> Rule: any operation that changes an aggregate's state goes through the root. No code outside the aggregate may modify its internals.

Aggregates in the codebase:

| Context | Aggregate Root | File |
| :-- | :-- | :-- |
| Identity | `User` | `src/identity-service/Identity.Domain/Aggregates/User.cs` |
| Catalog | `Product` | `src/catalog-service/Catalog.Domain/Aggregates/Product.cs` |
| **Sales (CORE)** | **`ShoppingCart`** | `src/sales-service/Sales.Domain/Aggregates/ShoppingCart.cs` |

`ShoppingCart` contains a list of `CartItem`s, but `CartItem`'s constructor is `internal` so no one outside the Domain assembly can build one — you can only get items by going through the root's `AddItem()` method. That's how the invariants are guaranteed.

The shared base class `AggregateRoot<TId>` (in each service's `Domain/Common/AggregateRoot.cs`) gives every root a `DomainEvents` list and a `RecordEvent` method. **The aggregate emits events; no one else does.**

### 4.2 Entity

An object whose identity matters more than its attributes. Two entities with the same identity are the same thing, even if their data differs.

> Rule: entities have an `Id` and equality is based on `Id`, not on the fields.

The base class `Entity<TId>` (in `Domain/Common/Entity.cs` of each service) implements this: `Equals` only compares IDs.

`CartItem` (`src/sales-service/Sales.Domain/Aggregates/CartItem.cs`) is the main non-root entity in Sales — its quantity can change without replacing the line. Its identity (`CartItemId`) is only meaningful **inside the owning `ShoppingCart`**.

### 4.3 Value Object

An immutable object defined by its data, not by an identity. Two value objects with the same fields are interchangeable.

> Rule: VOs validate their own invariants in the constructor and throw `DomainException` if violated. Once constructed, they cannot be mutated — every operation returns a new instance.

This is where most of the business rules live. Examples:

| VO | File | What its constructor enforces |
| :-- | :-- | :-- |
| `Money` | `Sales.Domain/ValueObjects/Money.cs` | `Amount >= 0`; currency must be a valid enum value |
| `Quantity` | `Sales.Domain/ValueObjects/Quantity.cs` | `Value >= 1` |
| `Email` | `Identity.Domain/ValueObjects/Email.cs` | matches regex; normalised to lowercase |
| `StockLevel` | `Catalog.Domain/ValueObjects/StockLevel.cs` | `OnHand >= 0`; `Reserve(qty)` throws if `OnHand < qty` |
| `ProductReference` | `Sales.Domain/ValueObjects/ProductReference.cs` | takes a **snapshot** of price+name — see §6 |
| `DiscountPolicy` (abstract) | `Sales.Domain/ValueObjects/Discounts/DiscountPolicy.cs` | base for `PercentageDiscount`, `FixedAmountDiscount`, `CompositeDiscount` |

The shared base `ValueObject` (`Domain/Common/ValueObject.cs`) implements structural equality: subclasses just override `GetEqualityComponents()`. That's how `new Money(10, USD) == new Money(10, USD)` returns `true` even though they're different references.

**Why this matters for defending the design:** every business rule about quantities, prices, emails, stock — they're enforced once, in the type. You **can't construct an invalid `Quantity`**. Compare to the legacy code where every method that took an `int qty` had to check `if (qty < 1)`. With VOs the check exists once and the type system carries the guarantee.

### 4.4 Domain Event

A fact that just happened in the domain, named in **past tense**, in **business language**.

> Rule: events are records emitted by the aggregate after a successful state change. They are immutable and carry only what subscribers need to know.

The seven events of the Sales context (the only ones we publish today) live in `src/sales-service/Sales.Domain/Events/`:

| Event | When the aggregate records it |
| :-- | :-- |
| `CarritoItemAgregado` | `AddItem()` succeeded |
| `CarritoItemRemovido` | `RemoveItem()` succeeded |
| `DescuentoAplicado` | `ApplyDiscount()` succeeded |
| `CheckoutIniciado` | `Checkout()` succeeded — crosses to Finance |
| `VentaCerrada` | `ConfirmSale()` succeeded — after `PagoAprobado` |
| `CheckoutRevertido` | `RevertCheckout()` succeeded — after `PagoRechazado` |
| `CarritoAbandonado` | `Abandon()` (session expiry) |

All of them implement the marker interface `IDomainEvent` (`Sales.Domain/Common/IDomainEvent.cs`) and are stored in the aggregate's `_domainEvents` list. They're flushed to the Outbox table (not published directly!) by the infrastructure — see §6.5 below.

**Naming convention** — Spanish past tense, because the ubiquitous language of this project is Spanish ("se agregó un ítem al carrito"). If we renamed it to `ItemAddedToCart` we'd be inventing a parallel English vocabulary the team doesn't speak.

### 4.5 Domain Service

A piece of business logic that **doesn't naturally fit inside one aggregate or VO** — typically because it coordinates multiple aggregates, or because it produces a new object that doesn't belong to any of them.

> Rule: domain services contain *business logic*; **application services** orchestrate use cases but contain no business logic. Easy to confuse — the line is "is this answering a domain question?" If yes, domain service.

In `src/sales-service/Sales.Domain/Services/`:

- **`CartTransitionService`** — turns a checked-out `ShoppingCart` into an immutable `OrderPlaced` record that gets handed to Finance. The transition produces a new aggregate-foreign object, so it can't live inside the cart.
- **`CartPricingService`** — composes multiple `DiscountPolicy` instances when they come from heterogeneous sources (VIP customer + promo coupon). The composition rule crosses policies, so it doesn't belong inside any one of them.

---

## 5. Ubiquitous Language — the glossary that binds code to business

Every identifier in the code must match this table. If you find a `processCart()` somewhere, that's a smell.

| Business term (Spanish) | Code term (English class names) | Definition |
| :-- | :-- | :-- |
| Cliente / Usuario | `User` / `CustomerId` | Registered person with credentials |
| Carrito | `ShoppingCart` (Sales aggregate root) | Mutable bag of items being negotiated |
| Ítem del carrito | `CartItem` (Sales entity) | One line: product snapshot + quantity |
| Producto | `Product` (Catalog aggregate root) | Item published for sale |
| Existencias | `StockLevel` (Catalog VO) | Available units |
| Política de descuento | `DiscountPolicy` (Sales VO, polymorphic) | Rule that turns subtotal → discounted amount |
| Subtotal | derived `Money` | Sum of line subtotals before discount |
| Total | `Money` | Subtotal after discount |
| Checkout | `Checkout()` (method on `ShoppingCart`) | Transition from negotiation to "ready to pay" |
| Pago | `Payment` (Finance aggregate root) | Monetary transaction |
| Pasarela de pago | external system reached **only** via the ACL | Third-party authoriser |
| Factura | `Invoice` (Finance aggregate root) | Legal record of a paid sale |

If the professor asks "why this name?" — the answer is *because that's what the business calls it*. Renaming any of these would be a step backward.

---

## 6. The Sales Core in detail (this is your strongest demo)

### 6.1 The aggregate and its invariants

`ShoppingCart` (`src/sales-service/Sales.Domain/Aggregates/ShoppingCart.cs`) guards five invariants:

1. **Product uniqueness** — two `CartItem`s can never share a `ProductId`. `AddItem()` consolidates quantities instead of duplicating lines.
2. **Positive quantities** — guaranteed by the `Quantity` VO (`Value >= 1`).
3. **Non-negative total** — `Money` rejects negative amounts in its constructor.
4. **Discount ≤ subtotal** — `ApplyDiscount()` recomputes the resulting total and throws if it would exceed the original subtotal.
5. **Price immutability** — once a product is in the cart, a later catalog price change does **not** mutate the line. This is enforced by storing a `ProductReference` (snapshot), not a live reference.

These invariants are why everything else exists. They're not optional checks — they're properties the type system carries.

### 6.2 Why `ProductReference` (snapshot) is the most important VO in the project

Look at `src/sales-service/Sales.Domain/ValueObjects/ProductReference.cs`. It holds `ProductId`, a copy of the name, and a copy of the price **at the moment the item was added**. The cart never goes back to ask Catalog "what's the price?" — that information is frozen.

Why this matters:
- A price change in Catalog after the cart is half-built **cannot retroactively change the total**.
- The cart remains coherent even if `catalog-service` is down — you can still show it, still check it out (catalog only comes back into play for the stock reservation).
- This is an **implicit Anti-Corruption Layer** — Catalog's model doesn't leak into Sales' model at runtime.

When defending this: this single decision crosses three concerns — business correctness (no retroactive surprises), availability (cart works during Catalog outages), and bounded-context isolation. It's not a trick, it's the kind of thing DDD is supposed to surface.

### 6.3 Discounts as a Value-Object hierarchy

`DiscountPolicy` (abstract) → three concrete VOs in `Sales.Domain/ValueObjects/Discounts/`:

- `PercentageDiscount(percent)` — `[0, 100]`
- `FixedAmountDiscount(amount)` — clamps to zero so the total never goes negative
- `CompositeDiscount(IEnumerable<DiscountPolicy>)` — applies its children in sequence (Composite pattern)

`ShoppingCart.ApplyDiscount(policy)` takes any of them. Adding a fourth discount type tomorrow (buy-one-get-one) means writing one new VO — the aggregate code doesn't change. That's the **Open/Closed Principle** dropping out of correct DDD, not a separate effort.

### 6.4 The seven triggers and seven events (rubric requirement)

This is the table the rubric specifically asks for (DDD criterion #6). All of these come from the actual code:

| Trigger (business event) | Command (Application layer) | Aggregate method | Domain event recorded |
| :-- | :-- | :-- | :-- |
| Customer adds a product | `AddItemToCartCommand` | `AddItem()` | `CarritoItemAgregado` |
| Customer removes a product | `RemoveItemFromCartCommand` | `RemoveItem()` | `CarritoItemRemovido` |
| System applies a discount | `ApplyDiscountCommand` | `ApplyDiscount()` | `DescuentoAplicado` |
| Customer confirms checkout | `CheckoutCommand` | `Checkout()` | `CheckoutIniciado` (→ Finance) |
| Finance approves | (notification `PagoAprobado`) | `ConfirmSale()` | `VentaCerrada` |
| Finance rejects | (notification `PagoRechazado`) | `RevertCheckout()` | `CheckoutRevertido` |
| Session expires | `ExpireCartCommand` | `Abandon()` | `CarritoAbandonado` |

Commands live under `src/sales-service/Sales.Application/Commands/`; aggregate methods are on `ShoppingCart`; events are in `Sales.Domain/Events/`. The mapping is 1-to-1 and traceable.

### 6.5 How events leave the domain — Outbox pattern (briefly)

Domain events recorded on the aggregate are not published directly by domain code. The infrastructure does it, transactionally:

1. The Application command handler invokes the aggregate method → events get added to `cart.DomainEvents`.
2. The `TransactionBehavior` (pipeline) calls `IUnitOfWork.CommitAsync()`.
3. The implementation of that (in `Sales.Infrastructure/Persistence/SalesDbContext.cs`) **drains** the events into the `outbox_messages` table in the same DB transaction.
4. A background `OutboxRelay` polls that table and publishes to RabbitMQ.

This pattern (Transactional Outbox) is the bridge between the DDD world (events emitted by aggregates) and the distributed-systems world (events crossing process boundaries). It's documented in detail in [`microservices_explained.md`](microservices_explained.md) §5.

---

## 7. The Anti-Corruption Layer — protecting the Core (rubric-critical)

The professor's grading rubric says: *"if there are corrupted domains, the implementation score is 0 points"*. The ACL is what guarantees the Sales Core is never corrupted by Finance's model.

### 7.1 What gets translated

Two directions, two mappers, all in `src/sales-service/Sales.Infrastructure/Acl/`:

**Outbound — Sales speaks Sales, Finance hears Finance:**

```
OrderPlaced  (Sales domain object)
   │
   ▼  FinanceAclMapper.ToPaymentRequest()
PaymentRequestMessage  (Finance vocabulary: transactionReference, currencyCode, paymentMethod, items[])
   │
   ▼  Outbox → RabbitMQ → finance-service
```

**Inbound — Finance speaks Finance, Sales hears Sales:**

```
PaymentResultMessage  (Finance vocabulary: outcome="AUTHORIZED"/"DECLINED", declineReason)
   │
   ▼  FinanceAclMapper.ToNotification()
PagoAprobado | PagoRechazado  (Sales integration events — already in the ubiquitous language)
   │
   ▼  MediatR.Publish → PagoAprobadoHandler.Handle()
cart.ConfirmSale()  (domain operation, no Finance type in sight)
```

### 7.2 Where you can see this in the code

| File | Role |
| :-- | :-- |
| `Sales.Infrastructure/Acl/Contracts/PaymentRequestMessage.cs` | Finance's outbound shape — confined to the ACL folder |
| `Sales.Infrastructure/Acl/Contracts/PaymentResultMessage.cs` | Finance's inbound shape — same |
| `Sales.Infrastructure/Acl/FinanceAclMapper.cs` | The bidirectional translator |
| `Sales.Infrastructure/Acl/FinancePaymentGateway.cs` | Implements `IPaymentGatewayService` (an Application port); ACL-translates the order and stages the request to the Outbox |
| `Sales.Application/IntegrationEvents/PagoAprobado.cs` | The translated Sales-side event the ACL produces |

**Key argument for the defense:** no Finance type (`PaymentRequestMessage`, `PaymentResultMessage`) is referenced anywhere in `Sales.Domain` or `Sales.Application`. You can `grep -r "PaymentRequestMessage" src/sales-service/Sales.Domain` and `grep -r "PaymentRequestMessage" src/sales-service/Sales.Application` — both return nothing. The ACL truly is a wall.

---

## 8. The folder structure mirrors the design — exactly

This is the most direct answer to *"under domain/ all the business is mixed together"*. Each service is organized first by Bounded Context (one folder per service), then by **layer** inside each. Inside `Domain/`, things are further organized by tactical pattern.

```
src/
├── sales-service/                  ◄── BC: Sales / Cart (CORE)
│   ├── Sales.Domain/               ← pure C#, zero dependencies
│   │   ├── Common/                 — Entity, ValueObject, AggregateRoot, IDomainEvent, DomainException
│   │   ├── Aggregates/             — ShoppingCart, CartItem, CartStatus
│   │   ├── ValueObjects/           — Money, Quantity, ProductReference, CartId, CustomerId, …
│   │   │   └── Discounts/          — DiscountPolicy + 3 concrete VOs (Composite pattern)
│   │   ├── Events/                 — 7 domain events (past-tense Spanish)
│   │   ├── Services/               — CartTransitionService, CartPricingService (domain services)
│   │   └── Interfaces/             — ICartRepository, IEventPublisher (ports the domain owns)
│   ├── Sales.Application/          ← CQRS use cases, MediatR, FluentValidation; no infra
│   │   ├── Commands/               — one folder per use case: Command + Handler + Validator
│   │   ├── Queries/                — GetCartQuery
│   │   ├── Sagas/                  — CheckoutSagaOrchestrator (the choreographer)
│   │   ├── Behaviors/              — ValidationBehavior, TransactionBehavior (pipeline)
│   │   ├── IntegrationEvents/      — PagoAprobado/PagoRechazado (already-translated)
│   │   ├── Abstractions/           — IUnitOfWork, IStockReservationService, IPaymentGatewayService (ports)
│   │   └── Dtos/                   — CartDto, MoneyDto, DiscountSpecDto (query results)
│   ├── Sales.Infrastructure/       ← EF Core, RabbitMQ, ACL, REST client; the only layer that touches I/O
│   │   ├── Persistence/            — SalesDbContext, repositories, EF configurations, OutboxMessage
│   │   ├── Caching/                — CachedCartRepository (Decorator)
│   │   ├── Outbox/                 — OutboxRelay BackgroundService
│   │   ├── Messaging/              — RabbitMqEventPublisher, PaymentResultConsumer
│   │   ├── Acl/                    — FinanceAclMapper + Finance contract copies (ACL)
│   │   └── Rest/                   — CatalogStockClient (with resilience handler)
│   └── Sales.Api/                  ← ASP.NET Core: thin controllers, HTTP DTOs, exception handler
│
├── catalog-service/                ◄── BC: Catalog (Supporting)
│   ├── Catalog.Domain/             — Product aggregate, Sku/Money/StockLevel VOs
│   ├── Catalog.Application/        — Reserve/Release stock commands, Get/List queries
│   ├── Catalog.Infrastructure/     — EF + seeder (creates 3 demo products on first boot)
│   └── Catalog.Api/                — ProductsController, StockReservationsController
│
├── identity-service/               ◄── BC: Identity (Supporting)
│   ├── Identity.Domain/            — User aggregate, Email/PersonName/PhoneNumber VOs (regex validation in ctors)
│   ├── Identity.Application/       — Authenticate, RegisterCustomer commands
│   ├── Identity.Infrastructure/    — EF + PBKDF2 password verifier + seeder (creates demo customer)
│   └── Identity.Api/               — UsersController, AuthController, CustomersController
│
└── finance-service/                ◄── BC: Finance (Generic) — demo stub
    └── (single-project consumer that auto-approves payments after 2s)
```

### 8.1 What the dependency direction tells the professor

Look at the `<ProjectReference>` declarations:

- `Sales.Domain.csproj` → **no project references, no NuGet packages**. The Core has zero dependencies.
- `Sales.Application.csproj` → references `Sales.Domain`, adds MediatR and FluentValidation. No infrastructure libraries.
- `Sales.Infrastructure.csproj` → references both above; adds EF Core, RabbitMQ, etc.
- `Sales.Api.csproj` → references Application + Infrastructure.

**Dependencies always point inward toward the Domain.** The Domain depends on nothing; Infrastructure depends on Domain (not the other way around). This is the **Dependency Inversion Principle** at project-file level — the compiler itself enforces the layering.

You can prove it on stage: try to add a `using Microsoft.EntityFrameworkCore;` to any file in `Sales.Domain/` — it won't compile, because EF isn't referenced. That's not discipline, that's the structure.

---

## 9. How to defend the most likely questions

**"Why is `ShoppingCart` the root and `CartItem` is not directly accessible?"**
Because every invariant of the cart (uniqueness of product, total = subtotal − discount ≥ 0, discount ≤ subtotal) is a property of the **set** of items, not of any single item. If callers could mutate `CartItem` directly, none of those invariants could be guaranteed. The `internal` constructor on `CartItem` is the compiler-enforced version of that rule.

**"Why is Finance Generic and not Supporting?"**
Because cobrar/facturar is a problem the industry has already solved — there are dozens of payment-gateway and electronic-invoicing SaaS providers. Eventually we'd replace the finance-service with a Stripe adapter; the ACL is what lets us do that without rewriting Sales. If it were Supporting, we'd be implying it's something we want to grow ourselves, which contradicts the business decision.

**"Why is the `Email` validator inside the `Email` VO and not a separate `EmailValidator` class?"**
Because a `string` shouldn't be allowed to flow through the system as a not-yet-validated email. If the validation lives in a separate class, anyone can forget to call it; if it lives in the constructor, **you literally cannot construct an invalid `Email`**. The type itself carries the guarantee. (Same reason `Quantity`, `Money`, `StockLevel` work this way.)

**"Why Spanish event names?"**
The ubiquitous language is Spanish ("se agregó al carrito"). Naming the event `ItemAddedToCart` would introduce a parallel English vocabulary the business team doesn't use, creating exactly the translation gap DDD is meant to eliminate.

**"Why does the cart store a `ProductReference` (snapshot) instead of a `Product`?"**
Three reasons in one decision: (a) price changes in Catalog must not retroactively change carts already in flight; (b) the cart remains usable when Catalog is down; (c) it implements an implicit anti-corruption layer between the Catalog and Sales domain models. This is, in my opinion, the most elegant single decision in the design.

**"Where is the application-layer business logic?"**
Trick question — **there isn't any**. Application services only orchestrate (load aggregate → invoke method → save). All business logic is in the aggregate or its VOs. The professor's rubric says *"orchestration, does not contain business logic"*; that's exactly what `CheckoutSagaOrchestrator.cs` does — five lines of orchestration, zero rules.

**"How do you guarantee transactional consistency across domain events and DB writes?"**
Transactional Outbox: events are written to an `outbox_messages` table in the same DB transaction as the business change (`SalesDbContext.CommitAsync` does both in one `SaveChangesAsync`). A separate background relay then publishes them. So the event reaches the broker **if and only if** the business change committed. See [`microservices_explained.md`](microservices_explained.md) §5 for the full pattern.

---

## 10. What to point at during the demo

When the professor wants to see the design **in code**, point to these files in this order — each one is 50–100 lines, easy to read on screen:

1. `src/sales-service/Sales.Domain/Aggregates/ShoppingCart.cs` — the aggregate root with all 5 invariants in 200 lines of pure C#.
2. `src/sales-service/Sales.Domain/ValueObjects/Money.cs` — a VO that enforces "no negative money" and rejects mixed-currency arithmetic.
3. `src/sales-service/Sales.Domain/Events/CheckoutIniciado.cs` — a domain event named in business language.
4. `src/sales-service/Sales.Domain/Sales.Domain.csproj` — *the proof* that the Core has zero dependencies.
5. `src/sales-service/Sales.Application/Commands/Checkout/CheckoutCommandHandler.cs` — a Hello-World-sized command handler that delegates to the orchestrator.
6. `src/sales-service/Sales.Infrastructure/Acl/FinanceAclMapper.cs` — the ACL: 40 lines that bridge two vocabularies.

If they ask "show me the rubric requirement", `ddd_explained.md` §4 and §6 are direct answers to rubric items #1–#7.
