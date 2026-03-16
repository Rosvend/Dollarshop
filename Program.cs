using TiendaOnline.Abstractions;
using TiendaOnline.Domain;
using TiendaOnline.Presentation;
using TiendaOnline.Services;

// --- Composition Root: wire all dependencies ---

// Repositories
var userRepo = new InMemoryUserRepository();
var productRepo = new InMemoryProductRepository();

// Seed a default user so login works out of the box
userRepo.Register(new RegistrationData
{
    Nombre = "Samuel",
    Apellido = "Default",
    Email = "samuel",
    Telefono = "0000000000"
}).PasswordHash = "123";

// Seed products (same catalog as the original)
productRepo.Seed(new List<Product>
{
    new Product { Id = 1, Name = "Portatil",        Price = 1000m, StockQuantity = 1 },
    new Product { Id = 2, Name = "Celular",         Price = 550m,  StockQuantity = 1 },
    new Product { Id = 3, Name = "Airpods",          Price = 80m,   StockQuantity = 1 },
    new Product { Id = 4, Name = "Tarjeta Grafica", Price = 800m,  StockQuantity = 1 },
    new Product { Id = 5, Name = "Mouse",            Price = 22.9m, StockQuantity = 1 },
    new Product { Id = 6, Name = "Parlantes",        Price = 300m,  StockQuantity = 1 },
    new Product { Id = 7, Name = "Bose",             Price = 400m,  StockQuantity = 1 },
});

// Services
IAuthenticationService authService = new AuthenticationService(userRepo);
var discount = new CompositeDiscount(new IDiscountStrategy[]
{
    new PercentageDiscount(10),
    new FixedAmountDiscount(5)
});
ICart cart = new ShoppingCart(discount);
IInvoiceService invoiceService = new InvoiceService();

// Payment strategies (OCP: add new methods here without modifying existing code)
var paymentService = new PaymentService(new IPaymentMethod[]
{
    new CardPayment(),
    new CashPayment(),
    new AccountPayment()
});

// Validators (ISP: each validator has a single focused contract)
IValidator<string> nameValidator = new NameValidator();
IValidator<string> emailValidator = new EmailValidator();
IValidator<string> phoneValidator = new PhoneValidator();

// Facade: CheckoutService wraps payment + invoice orchestration
ICheckoutService checkoutService = new CheckoutService(paymentService, invoiceService);

// --- Launch the application ---
var ui = new ConsoleUI(
    authService,
    userRepo,
    productRepo,
    cart,
    checkoutService,
    nameValidator,
    emailValidator,
    phoneValidator
);

ui.Run();
