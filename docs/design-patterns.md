# Design Patterns Analysis — Dollarshop

## 1. Introduction

Dollarshop is a C# .NET 10 console e-commerce application built with a clean four-layer SOLID architecture:

| Layer | Directory | Responsibility |
|-------|-----------|---------------|
| **Presentation** | `Presentation/` | User interaction (`ConsoleUI`) |
| **Abstractions** | `Abstractions/` | Contracts and interfaces (8 interfaces) |
| **Services** | `Services/` | Business logic, repositories, strategies (14 classes) |
| **Domain** | `Domain/` | Plain entities and DTOs (6 classes) |

Dependencies flow inward: Presentation → Abstractions ← Services → Domain. The composition root in `Program.cs` wires everything together using manual constructor injection.

This document analyzes all **23 Gang of Four (GoF) design patterns**, evaluating whether each one is applicable to the project, where it would be implemented, and why.

---

## 2. Already Implemented Patterns

### Strategy Pattern

The project uses the Strategy pattern in two places:

- **Payment strategies** — `IPaymentMethod` (`Abstractions/IPaymentMethod.cs`) with three implementations: `CardPayment`, `CashPayment`, `AccountPayment` (all in `Services/`). The `PaymentService` class selects and delegates to the appropriate strategy at runtime via its `Process()` method.
- **Discount strategies** — `IDiscountStrategy` (`Abstractions/IDiscountStrategy.cs`) with two implementations: `FixedAmountDiscount` and `PercentageDiscount` (both in `Services/`). The `ShoppingCart` class applies the discount in `CalculateTotal()`.

### Repository Pattern

- `IProductRepository` → `InMemoryProductRepository`
- `IUserRepository` → `InMemoryUserRepository`

Both repositories abstract data access behind interfaces, allowing the in-memory implementations to be swapped for database-backed ones without changing any consumer code.

---

## 3. Creational Patterns

### 3.1 Factory Method

| | |
|---|---|
| **Description** | Defines an interface for creating objects, letting subclasses decide which class to instantiate. |
| **Verdict** | **Recommended** |
| **Where** | `Services/` layer — create a `PaymentMethodFactory` to replace the manual instantiation in `Program.cs` (lines 39–44). |
| **Reasoning** | Currently, `Program.cs` manually creates `CardPayment`, `CashPayment`, and `AccountPayment` and passes them to `PaymentService`. A factory method could encapsulate payment method creation, making it easier to add new payment types (e.g., crypto, PayPal) without modifying the composition root. The same approach could apply to validators — a `ValidatorFactory` that returns the appropriate `IValidator<string>` based on the field being validated. |

### 3.2 Abstract Factory

| | |
|---|---|
| **Description** | Provides an interface for creating families of related objects without specifying concrete classes. |
| **Verdict** | **Not Recommended** |
| **Where** | N/A |
| **Reasoning** | Abstract Factory is useful when a system needs to produce families of related objects (e.g., a UI toolkit that creates buttons, scrollbars, and windows for different OS themes). Dollarshop's object families (payments, discounts, validators) are independent of each other — there is no scenario where switching one family requires switching another. The simpler Factory Method pattern covers the project's needs. |

### 3.3 Builder

| | |
|---|---|
| **Description** | Separates the construction of a complex object from its representation, allowing the same construction process to create different representations. |
| **Verdict** | **Recommended** |
| **Where** | `Services/InvoiceService.cs` — introduce an `InvoiceBuilder` to construct `Invoice` objects. |
| **Reasoning** | The `InvoiceService.Generate()` method creates an `Invoice` by setting multiple properties at once (Id, CustomerNumber, Items, Total, Date, PaymentMethod). As the invoice grows in complexity (e.g., adding tax calculations, shipping info, discounts breakdown, header/footer), a builder would make construction more readable and flexible. Example: `new InvoiceBuilder().WithCustomer(id).WithItems(cart.GetItems()).WithPayment(method).Build()`. This also applies to `RegistrationData` in the registration flow (`ConsoleUI.cs`, lines 102–131), where fields are collected step by step — a natural fit for the Builder pattern. |

### 3.4 Prototype

| | |
|---|---|
| **Description** | Creates new objects by cloning an existing instance (the prototype) rather than using constructors. |
| **Verdict** | **Not Recommended** |
| **Where** | N/A |
| **Reasoning** | The domain objects (`Product`, `User`, `CartItem`, `Invoice`) are simple POCOs with no expensive initialization. There is no scenario where cloning an existing object would be preferable to creating a new one. The `CartItem` class could theoretically be cloned when duplicating cart entries, but `new CartItem { ... }` is trivially simple. Prototype adds unnecessary complexity here. |

### 3.5 Singleton

| | |
|---|---|
| **Description** | Ensures a class has only one instance and provides a global access point to it. |
| **Verdict** | **Not Recommended** |
| **Where** | N/A |
| **Reasoning** | The project already achieves single-instance behavior through its composition root (`Program.cs`), which creates one instance of each service and shares it via constructor injection. This is the preferred approach in modern .NET — using DI container lifetime management instead of the Singleton pattern. Making classes like `AuthenticationService` or `InMemoryProductRepository` into singletons would introduce global state, tight coupling, and testing difficulties. The current manual DI approach is cleaner and more testable. |

---

## 4. Structural Patterns

### 4.1 Adapter

| | |
|---|---|
| **Description** | Converts the interface of a class into another interface that clients expect, allowing incompatible interfaces to work together. |
| **Verdict** | **Recommended** |
| **Where** | `Services/` layer — useful when integrating external systems. |
| **Reasoning** | If the project were to replace `InMemoryProductRepository` with a real database (e.g., Entity Framework, Dapper, or an external REST API), an adapter would translate the external system's interface into `IProductRepository`. For example, a `SqlProductRepositoryAdapter` that wraps a `DbContext` and exposes `GetAll()`, `FindById()`, and `UpdateStock()`. The same applies to `IUserRepository`. The existing interface-based architecture already supports this pattern — the in-memory implementations are effectively adapters over `List<T>`. |

### 4.2 Bridge

| | |
|---|---|
| **Description** | Decouples an abstraction from its implementation so that the two can vary independently. |
| **Verdict** | **Not Recommended** |
| **Where** | N/A |
| **Reasoning** | Bridge is useful when you have multiple dimensions of variation (e.g., shapes × rendering engines). Dollarshop's abstractions each have a single dimension of variation — `IPaymentMethod` varies by payment type, `IDiscountStrategy` varies by discount logic. There is no need to combine orthogonal hierarchies. The existing Strategy pattern already provides the needed flexibility. |

### 4.3 Composite

| | |
|---|---|
| **Description** | Composes objects into tree structures to represent part-whole hierarchies, letting clients treat individual objects and compositions uniformly. |
| **Verdict** | **Recommended** |
| **Where** | `Services/` layer — apply to `IDiscountStrategy`. |
| **Reasoning** | Currently, `ShoppingCart` accepts a single `IDiscountStrategy`. With Composite, you could create a `CompositeDiscount : IDiscountStrategy` that holds a list of `IDiscountStrategy` instances and applies them sequentially (e.g., a 10% loyalty discount **and** a $5 coupon). This lets `ShoppingCart` remain unchanged — it still calls `Apply()` on one object — while supporting combinations of discounts. Implementation: a `CompositeDiscount` class in `Services/` that implements `IDiscountStrategy` and iterates over child strategies. |

### 4.4 Decorator

| | |
|---|---|
| **Description** | Attaches additional responsibilities to an object dynamically by wrapping it in a decorator that implements the same interface. |
| **Verdict** | **Recommended** |
| **Where** | `Services/` layer — wrap `ICart`, `IPaymentMethod`, or `IProductRepository`. |
| **Reasoning** | Decorator is a strong fit for adding cross-cutting concerns without modifying existing classes. Concrete examples: (1) A `LoggingCartDecorator : ICart` that logs every `AddItem()` and `RemoveItem()` call before delegating to the real `ShoppingCart`. (2) A `CachingProductRepositoryDecorator : IProductRepository` that caches `GetAll()` results. (3) A `PaymentLoggingDecorator : IPaymentMethod` that records payment attempts for auditing. Since all services are coded to interfaces, decorators can be injected transparently in `Program.cs`. |

### 4.5 Facade

| | |
|---|---|
| **Description** | Provides a simplified interface to a complex subsystem, shielding clients from its internal details. |
| **Verdict** | **Recommended** |
| **Where** | `Services/` layer — introduce a `CheckoutFacade` to simplify the checkout flow. |
| **Reasoning** | The `ConsoleUI.HandlePayment()` method (lines 367–414) orchestrates multiple services: it reads the payment method, processes the payment via `PaymentService`, generates an invoice via `InvoiceService`, and clears the cart. This multi-step coordination belongs in a `CheckoutFacade` (or `CheckoutService`) that exposes a single `Checkout(cart, paymentInfo)` method. This would reduce `ConsoleUI`'s responsibilities (currently 514 lines) and make the checkout logic reusable and testable independently of the UI. The existing `PaymentService` is already a mini-facade over multiple `IPaymentMethod` implementations. |

### 4.6 Flyweight

| | |
|---|---|
| **Description** | Uses sharing to support large numbers of fine-grained objects efficiently by externalizing shared state. |
| **Verdict** | **Not Recommended** |
| **Where** | N/A |
| **Reasoning** | Flyweight optimizes memory when thousands of similar objects share intrinsic state (e.g., character glyphs in a text editor). Dollarshop works with a small number of `Product` objects (7 seeded items), `User` objects, and `CartItem` objects. There is no memory pressure that would justify the added complexity of separating intrinsic from extrinsic state. |

### 4.7 Proxy

| | |
|---|---|
| **Description** | Provides a surrogate or placeholder for another object to control access to it. |
| **Verdict** | **Recommended** |
| **Where** | `Services/` layer — add access control to repositories or services. |
| **Reasoning** | A protection proxy could enforce authorization checks. For example, a `SecureProductRepository : IProductRepository` that verifies the current user is authenticated (via `IAuthenticationService`) before allowing `UpdateStock()` calls. Currently, stock updates in `ConsoleUI` (lines 261–290) rely on the UI to enforce access control. Moving this responsibility to a proxy would ensure security at the service level regardless of the caller. A virtual proxy could also add lazy loading for product catalogs if the data source becomes a database. |

---

## 5. Behavioral Patterns

### 5.1 Chain of Responsibility

| | |
|---|---|
| **Description** | Passes a request along a chain of handlers, where each handler decides whether to process the request or pass it to the next handler. |
| **Verdict** | **Recommended** |
| **Where** | `Services/` layer — refactor validation into a chain. |
| **Reasoning** | The registration flow in `ConsoleUI.HandleRegistration()` (lines 102–131) validates name, email, and phone sequentially using separate `IValidator<string>` instances. These validators could be linked into a chain where each validator either passes the data through or rejects it with an error. This would allow dynamic composition of validation rules (e.g., adding a password-strength validator to the chain) without modifying the calling code. Each validator would implement a `Handle(string value, ValidationContext context)` method and optionally delegate to the next validator. |

### 5.2 Command

| | |
|---|---|
| **Description** | Encapsulates a request as an object, allowing parameterization, queuing, logging, and undo operations. |
| **Verdict** | **Recommended** |
| **Where** | `Presentation/` and `Services/` layers — encapsulate cart operations as commands. |
| **Reasoning** | Cart operations (`AddItem`, `RemoveItem`) and checkout could be modeled as command objects: `AddToCartCommand`, `RemoveFromCartCommand`, `CheckoutCommand`. This enables: (1) **Undo** — undoing the last cart operation by reversing the command. (2) **History** — maintaining a log of all user actions. (3) **Decoupling** — `ConsoleUI` menu handlers would create and execute command objects instead of calling services directly, reducing the 514-line class. Each command would implement an `ICommand` interface with `Execute()` and `Undo()` methods. |

### 5.3 Iterator

| | |
|---|---|
| **Description** | Provides a way to access elements of a collection sequentially without exposing its underlying representation. |
| **Verdict** | **Not Recommended** |
| **Where** | N/A |
| **Reasoning** | C# already provides the `IEnumerable<T>` / `IEnumerator<T>` interfaces and LINQ, which are the language's built-in implementation of the Iterator pattern. The project already uses these — `ShoppingCart` internally uses `List<CartItem>` with LINQ (`FirstOrDefault`, `Sum`), and `InMemoryProductRepository` returns `List<Product>`. Implementing a custom iterator would duplicate what the framework already provides. The pattern is present implicitly through the language runtime. |

### 5.4 Mediator

| | |
|---|---|
| **Description** | Defines an object that encapsulates how a set of objects interact, promoting loose coupling by preventing objects from referring to each other directly. |
| **Verdict** | **Recommended** |
| **Where** | `Services/` layer — introduce a mediator between `ConsoleUI` and the service layer. |
| **Reasoning** | `ConsoleUI` currently holds references to 9 dependencies (lines 19–39): `IAuthenticationService`, `IUserRepository`, `IProductRepository`, `ICart`, `PaymentService`, `IInvoiceService`, and 3 validators. A `ShopMediator` could centralize the coordination between these services, reducing `ConsoleUI`'s coupling. Instead of calling each service directly, `ConsoleUI` would send requests (e.g., `mediator.Send(new AddToCartRequest(productId, qty))`) and the mediator would route them to the appropriate service. This aligns with the CQRS-lite approach common in .NET projects (similar to MediatR). |

### 5.5 Memento

| | |
|---|---|
| **Description** | Captures and externalizes an object's internal state so it can be restored later, without violating encapsulation. |
| **Verdict** | **Not Recommended** |
| **Where** | N/A |
| **Reasoning** | Memento is useful for implementing undo/redo or snapshots of complex state. While cart undo could be valuable (see Command pattern above), the cart state is simple enough that the Command pattern's `Undo()` method can reverse operations directly (e.g., re-add a removed item) without needing full state snapshots. The domain objects are small POCOs, and there is no editor-like interface where users would need to revert to arbitrary previous states. The overhead of capturing and storing mementos is not justified. |

### 5.6 Observer

| | |
|---|---|
| **Description** | Defines a one-to-many dependency between objects so that when one changes state, all dependents are notified and updated automatically. |
| **Verdict** | **Recommended** |
| **Where** | `Services/` layer — notify interested parties of domain events. |
| **Reasoning** | Several actions in the system could trigger notifications: (1) When a payment is processed successfully, an observer could generate the invoice automatically (currently done manually in `ConsoleUI.HandlePayment()`). (2) When stock is updated via `IProductRepository.UpdateStock()`, observers could check for low-stock alerts. (3) When a user registers, an observer could send a welcome notification. C# has built-in support via `event` and `EventHandler<T>`. For example, `PaymentService` could raise a `PaymentCompleted` event that `InvoiceService` subscribes to, removing the need for `ConsoleUI` to orchestrate the two. |

### 5.7 State

| | |
|---|---|
| **Description** | Allows an object to alter its behavior when its internal state changes, appearing to change its class. |
| **Verdict** | **Recommended** |
| **Where** | `Presentation/ConsoleUI.cs` — model the application's menu navigation as states. |
| **Reasoning** | `ConsoleUI` manages multiple menu states: Welcome (login/register), Main Menu, Client Menu, Product Menu, Cart Menu. Currently, each state is a method (`ShowWelcomeMenu`, `ShowMainMenu`, `ShowClientMenu`, etc.) with switch statements. With the State pattern, each menu would be a class implementing an `IMenuState` interface with a `Handle()` method. `ConsoleUI` would hold a current state and delegate to it. This eliminates the nested switch-case logic, makes each menu independently testable, and simplifies adding new menus. Transitions would be explicit: `SetState(new CartMenuState(this))`. |

### 5.8 Strategy

| | |
|---|---|
| **Description** | Defines a family of algorithms, encapsulates each one, and makes them interchangeable. |
| **Verdict** | **Already Implemented** |
| **Where** | `Abstractions/IPaymentMethod.cs` → `Services/CardPayment.cs`, `CashPayment.cs`, `AccountPayment.cs`; `Abstractions/IDiscountStrategy.cs` → `Services/FixedAmountDiscount.cs`, `PercentageDiscount.cs`. |
| **Reasoning** | See [Section 2](#2-already-implemented-patterns). The pattern is well-implemented with clean interface segregation and constructor injection. `PaymentService.Process()` dynamically selects the payment strategy, and `ShoppingCart.CalculateTotal()` delegates to the discount strategy. No changes needed. |

### 5.9 Template Method

| | |
|---|---|
| **Description** | Defines the skeleton of an algorithm in a base class, letting subclasses override specific steps without changing the algorithm's structure. |
| **Verdict** | **Recommended** |
| **Where** | `Services/` layer — extract a base class for validators or payment methods. |
| **Reasoning** | The three validators (`NameValidator`, `EmailValidator`, `PhoneValidator`) follow the same structure: check for null/whitespace, then apply a regex. A base class `RegexValidator : IValidator<string>` could define the template: `IsValid()` checks `!string.IsNullOrWhiteSpace(value)` and then calls an abstract `GetPattern()` method. Each concrete validator only provides its regex pattern and error message. This eliminates the duplicated null-check logic across all three validators. Similarly, `CardPayment` and `AccountPayment` share identical `Pay()` logic (check `AccountNumber` + `amount > 0`) — a `BasePayment` class could extract this common validation step. |

### 5.10 Visitor

| | |
|---|---|
| **Description** | Lets you define new operations on elements of an object structure without changing the classes on which it operates. |
| **Verdict** | **Not Recommended** |
| **Where** | N/A |
| **Reasoning** | Visitor is most useful with complex, stable object hierarchies where you need to add many operations (e.g., compilers applying different passes over an AST). Dollarshop's domain model is flat — `Product`, `User`, `CartItem`, `Invoice` are independent POCOs, not a composite hierarchy. Adding operations like "calculate tax" or "format for display" is better handled by dedicated service classes (`InvoiceService`, validators) than by adding `Accept(IVisitor)` methods to every domain object. The pattern would add significant boilerplate for minimal benefit. |

---

## 6. Summary Table

| # | Pattern | Category | Verdict | Applicable Area |
|---|---------|----------|---------|----------------|
| 1 | Factory Method | Creational | **Recommended** | Payment/validator creation in `Program.cs` |
| 2 | Abstract Factory | Creational | Not Recommended | No related object families |
| 3 | Builder | Creational | **Recommended** | `Invoice` and `RegistrationData` construction |
| 4 | Prototype | Creational | Not Recommended | Simple POCOs, no cloning needed |
| 5 | Singleton | Creational | Not Recommended | DI already handles single instances |
| 6 | Adapter | Structural | **Recommended** | Repository implementations for external systems |
| 7 | Bridge | Structural | Not Recommended | No multi-dimensional variation |
| 8 | Composite | Structural | **Recommended** | Combine multiple `IDiscountStrategy` |
| 9 | Decorator | Structural | **Recommended** | Logging/caching wrappers for `ICart`, `IProductRepository` |
| 10 | Facade | Structural | **Recommended** | `CheckoutFacade` to simplify payment + invoice flow |
| 11 | Flyweight | Structural | Not Recommended | Small object count, no memory pressure |
| 12 | Proxy | Structural | **Recommended** | Authorization proxy for repositories |
| 13 | Chain of Responsibility | Behavioral | **Recommended** | Validation chain for registration |
| 14 | Command | Behavioral | **Recommended** | Cart operations with undo support |
| 15 | Iterator | Behavioral | Not Recommended | Already provided by C# `IEnumerable<T>` |
| 16 | Mediator | Behavioral | **Recommended** | Decouple `ConsoleUI` from 9 service dependencies |
| 17 | Memento | Behavioral | Not Recommended | Simple state, Command pattern suffices |
| 18 | Observer | Behavioral | **Recommended** | Payment/stock/registration events |
| 19 | State | Behavioral | **Recommended** | Menu navigation in `ConsoleUI` |
| 20 | Strategy | Behavioral | **Already Implemented** | Payments and discounts |
| 21 | Template Method | Behavioral | **Recommended** | Base class for validators and payments |
| 22 | Visitor | Behavioral | Not Recommended | Flat domain model, no composite hierarchy |
| 23 | — | — | **Repository (Already Impl.)** | `IProductRepository`, `IUserRepository` |

**Totals:** 13 Recommended | 2 Already Implemented | 8 Not Recommended
