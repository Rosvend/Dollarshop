using TiendaOnline.Abstractions;
using TiendaOnline.Domain;

namespace TiendaOnline.Presentation
{
    public class ConsoleUI
    {
        private readonly IAuthenticationService _authService;
        private readonly IUserRepository _userRepo;
        private readonly IProductRepository _productRepo;
        private readonly ICart _cart;
        private readonly ICheckoutService _checkoutService;
        private readonly IValidator<string> _nameValidator;
        private readonly IValidator<string> _emailValidator;
        private readonly IValidator<string> _phoneValidator;

        public ConsoleUI(
            IAuthenticationService authService,
            IUserRepository userRepo,
            IProductRepository productRepo,
            ICart cart,
            ICheckoutService checkoutService,
            IValidator<string> nameValidator,
            IValidator<string> emailValidator,
            IValidator<string> phoneValidator)
        {
            _authService = authService;
            _userRepo = userRepo;
            _productRepo = productRepo;
            _cart = cart;
            _checkoutService = checkoutService;
            _nameValidator = nameValidator;
            _emailValidator = emailValidator;
            _phoneValidator = phoneValidator;
        }

        public void Run()
        {
            ShowWelcomeMenu();
            ShowMainMenu();
        }

        private void ShowWelcomeMenu()
        {
            bool authenticated = false;

            while (!authenticated)
            {
                Console.WriteLine("-----BIENVENIDO-----\n");
                Console.WriteLine("1.Iniciar Sesion");
                Console.WriteLine("2.Registrarse\n");
                Console.Write("Seleccione una opcion: ");

                if (int.TryParse(Console.ReadLine(), out int opcion))
                {
                    switch (opcion)
                    {
                        case 1:
                            Console.WriteLine();
                            authenticated = HandleLogin();
                            break;
                        case 2:
                            Console.WriteLine();
                            authenticated = HandleRegistration();
                            break;
                        default:
                            Console.WriteLine("Opcion no valida. Por favor, vuelva a intentar.\n");
                            break;
                    }
                }
                else
                {
                    Console.WriteLine("Por favor ingrese solo numeros.\n");
                }
            }
        }

        private bool HandleLogin()
        {
            Console.Write("Ingrese su Usuario: ");
            string username = Console.ReadLine() ?? string.Empty;

            Console.Write("Ingrese su Contrasena: ");
            string password = Console.ReadLine() ?? string.Empty;

            if (_authService.Authenticate(username, password))
            {
                var user = _authService.GetCurrentUser();
                Console.WriteLine($"\nUsuario: {user!.Username}");
                Console.WriteLine("INICIO EXITOSO\n");
                return true;
            }

            Console.WriteLine("\nUsuario o contrasena incorrectas. Por favor, intentelo de nuevo.\n");
            return false;
        }

        private bool HandleRegistration()
        {
            string nombre = ReadValidated("Ingrese su nombre: ", _nameValidator);
            string apellido = ReadValidated("Ingrese su apellido: ", _nameValidator);
            string email = ReadValidated("Ingrese su email: ", _emailValidator);
            string telefono = ReadValidated("Ingrese su telefono: ", _phoneValidator);

            Console.Write("Ingrese una contrasena: ");
            string password = Console.ReadLine() ?? string.Empty;

            var data = new RegistrationData
            {
                Nombre = nombre,
                Apellido = apellido,
                Email = email,
                Telefono = telefono
            };

            var user = _userRepo.Register(data);
            user.PasswordHash = password;

            Console.WriteLine($"\nNombre: {data.Nombre}");
            Console.WriteLine($"Apellido: {data.Apellido}");
            Console.WriteLine($"Email: {data.Email}");
            Console.WriteLine($"Telefono: {data.Telefono}");
            Console.WriteLine("REGISTRO EXITOSO\n");

            _authService.Authenticate(user.Username, password);
            return true;
        }

        private string ReadValidated(string prompt, IValidator<string> validator)
        {
            while (true)
            {
                Console.Write(prompt);
                string input = Console.ReadLine() ?? string.Empty;

                if (validator.IsValid(input))
                {
                    return input;
                }

                Console.WriteLine(validator.ErrorMessage);
            }
        }

        private void ShowMainMenu()
        {
            int opcion;

            do
            {
                Console.WriteLine("-----MENU PRINCIPAL-----\n");
                Console.WriteLine("1.CLIENTE");
                Console.WriteLine("2.PRODUCTOS");
                Console.WriteLine("3.CARRITO");
                Console.WriteLine("0.SALIR\n");
                Console.Write("Ingrese una opcion: ");

                if (!int.TryParse(Console.ReadLine(), out opcion))
                {
                    Console.WriteLine("Por favor ingrese solo numeros.\n");
                    continue;
                }

                Console.WriteLine();

                switch (opcion)
                {
                    case 1:
                        ShowClientMenu();
                        break;
                    case 2:
                        ShowProductMenu();
                        break;
                    case 3:
                        ShowCartMenu();
                        break;
                    case 0:
                        Console.WriteLine("Gracias por su visita.\n");
                        break;
                    default:
                        Console.WriteLine("Ingrese una opcion valida.\n");
                        break;
                }
            } while (opcion != 0);
        }

        private void ShowClientMenu()
        {
            int opcion;

            do
            {
                Console.WriteLine("CLIENTE");
                Console.WriteLine("1.Ver Productos");
                Console.WriteLine("2.Ver Carrito");
                Console.WriteLine("0.Volver");
                Console.Write("Elija una opcion: ");

                if (!int.TryParse(Console.ReadLine(), out opcion))
                {
                    Console.WriteLine("Por favor ingrese solo numeros.\n");
                    continue;
                }

                Console.WriteLine();

                switch (opcion)
                {
                    case 1:
                        PrintProducts();
                        break;
                    case 2:
                        PrintCartItems();
                        break;
                    case 0:
                        break;
                    default:
                        Console.WriteLine("Ingrese una opcion valida.\n");
                        break;
                }
            } while (opcion != 0);
        }

        private void ShowProductMenu()
        {
            int opcion;

            do
            {
                Console.WriteLine("PRODUCTO");
                Console.WriteLine("1.Agregar al carrito");
                Console.WriteLine("0.Volver");
                Console.Write("Elija una opcion: ");

                if (!int.TryParse(Console.ReadLine(), out opcion))
                {
                    Console.WriteLine("Por favor ingrese solo numeros.\n");
                    continue;
                }

                Console.WriteLine();

                switch (opcion)
                {
                    case 1:
                        HandleAddToCart();
                        break;
                    case 0:
                        break;
                    default:
                        Console.WriteLine("Ingrese una opcion valida.\n");
                        break;
                }
            } while (opcion != 0);
        }

        private void HandleAddToCart()
        {
            PrintProducts();
            Console.WriteLine("Ingrese el ID del producto que desea agregar al carrito (0 para salir):");

            while (true)
            {
                Console.Write("> ");
                if (!int.TryParse(Console.ReadLine(), out int id))
                {
                    Console.WriteLine("Entrada invalida. Por favor, ingrese un numero valido o 0 para salir.");
                    continue;
                }

                if (id == 0)
                    break;

                var product = _productRepo.FindById(id);

                if (product == null || product.StockQuantity <= 0)
                {
                    Console.WriteLine("Producto no disponible, agregue otro producto.");
                    continue;
                }

                _cart.AddItem(product, 1);
                _productRepo.UpdateStock(id, -1);
                Console.WriteLine($"Producto '{product.Name}' agregado al carrito.");
            }
        }

        private void ShowCartMenu()
        {
            int opcion;

            do
            {
                Console.WriteLine("-----CARRITO-----\n");
                Console.WriteLine("PRODUCTOS EN EL CARRITO:");
                PrintCartItems();
                Console.WriteLine();
                Console.WriteLine("1.Sacar producto");
                Console.WriteLine("2.Calcular total");
                Console.WriteLine("3.Hacer pago");
                Console.WriteLine("0.Volver\n");
                Console.Write("Elija una opcion: ");

                if (!int.TryParse(Console.ReadLine(), out opcion))
                {
                    Console.WriteLine("Por favor ingrese solo numeros.\n");
                    continue;
                }

                Console.WriteLine();

                switch (opcion)
                {
                    case 1:
                        HandleRemoveFromCart();
                        break;
                    case 2:
                        Console.WriteLine($"Total del Carrito: {_cart.CalculateTotal():C}\n");
                        break;
                    case 3:
                        HandlePayment();
                        break;
                    case 0:
                        break;
                    default:
                        Console.WriteLine("Ingrese una opcion valida.\n");
                        break;
                }
            } while (opcion != 0);
        }

        private void HandleRemoveFromCart()
        {
            Console.WriteLine("Ingrese el ID del producto que desea sacar del carrito (0 para cancelar):");

            while (true)
            {
                Console.Write("> ");
                if (!int.TryParse(Console.ReadLine(), out int id))
                {
                    Console.WriteLine("Entrada invalida. Por favor, ingrese un numero valido.");
                    continue;
                }

                if (id == 0)
                    break;

                var items = _cart.GetItems();
                var item = items.FirstOrDefault(i => i.Product.Id == id);

                if (item == null)
                {
                    Console.WriteLine("Producto no encontrado en el carrito.");
                    continue;
                }

                _cart.RemoveItem(id);
                _productRepo.UpdateStock(id, item.Quantity);
                Console.WriteLine($"Producto '{item.Product.Name}' sacado del carrito.");
            }
        }

        private void HandlePayment()
        {
            if (_cart.GetItems().Count == 0)
            {
                Console.WriteLine("El carrito esta vacio. Agregue productos antes de pagar.\n");
                return;
            }

            Console.WriteLine("-----PAGAR-----\n");

            var methods = _checkoutService.GetAvailablePaymentMethods();
            string selectedMethod = ReadPaymentMethod(methods);

            string accountNumber = string.Empty;
            if (!selectedMethod.Equals("Efectivo", StringComparison.OrdinalIgnoreCase))
            {
                accountNumber = ReadAccountNumber();
            }

            decimal total = _cart.CalculateTotal();
            Console.WriteLine($"\nTotal a pagar: {total:C}");
            Console.WriteLine("\n1.Confirmar pago");
            Console.WriteLine("2.Cancelar\n");
            Console.Write("Elija una opcion: ");

            if (!int.TryParse(Console.ReadLine(), out int confirm) || confirm != 1)
            {
                Console.WriteLine("Pago cancelado.\n");
                return;
            }

            int customerNumber = ReadCustomerNumber();

            var paymentInfo = new PaymentInfo
            {
                Method = selectedMethod,
                AccountNumber = accountNumber
            };

            var result = _checkoutService.Process(_cart, paymentInfo, customerNumber);

            if (!result.Success)
            {
                Console.WriteLine("\nPago rechazado. Verifique sus datos.\n");
                return;
            }

            Console.WriteLine("\nPago ACEPTADO.\n");
            HandleInvoice(result.Invoice!);
        }

        private string ReadPaymentMethod(List<string> methods)
        {
            string methodsDisplay = string.Join(", ", methods);

            while (true)
            {
                Console.Write($"Ingrese el metodo de pago ({methodsDisplay}): ");
                string input = Console.ReadLine() ?? string.Empty;

                if (methods.Any(m => m.Equals(input, StringComparison.OrdinalIgnoreCase)))
                {
                    return input;
                }

                Console.WriteLine("METODO DE PAGO INVALIDO. Intentelo nuevamente.\n");
            }
        }

        private string ReadAccountNumber()
        {
            while (true)
            {
                Console.Write("Ingrese numero de cuenta: ");
                string input = Console.ReadLine() ?? string.Empty;

                if (!string.IsNullOrWhiteSpace(input) && input.All(char.IsDigit))
                {
                    return input;
                }

                Console.WriteLine("Por favor, ingrese solo numeros.");
            }
        }

        private void HandleInvoice(Invoice invoice)
        {
            Console.WriteLine("-----FACTURA-----\n");
            Console.WriteLine($"Identificacion: {invoice.CustomerNumber}");
            Console.WriteLine($"Total a pagar: {invoice.Total:C}");
            Console.WriteLine($"Fecha: {invoice.Date:dd-MM-yy}");
            Console.WriteLine($"Metodo de pago: {invoice.PaymentMethod}");
            Console.WriteLine();
            Console.WriteLine("-----SU RECIBO HA SIDO GENERADO-----\n");
        }

        private int ReadCustomerNumber()
        {
            while (true)
            {
                Console.Write("Ingrese su identificacion: ");
                if (int.TryParse(Console.ReadLine(), out int id))
                {
                    return id;
                }
                Console.WriteLine("Por favor, ingrese solo numeros.");
            }
        }

        private void PrintProducts()
        {
            Console.WriteLine("LISTA DE PRODUCTOS:");
            foreach (var p in _productRepo.GetAll())
            {
                Console.WriteLine($"  ID: {p.Id}, Nombre: {p.Name}, Precio: {p.Price:C}, Stock: {p.StockQuantity}");
            }
            Console.WriteLine();
        }

        private void PrintCartItems()
        {
            var items = _cart.GetItems();
            if (items.Count == 0)
            {
                Console.WriteLine("  (vacio)");
                return;
            }

            foreach (var item in items)
            {
                Console.WriteLine($"  ID: {item.Product.Id}, Nombre: {item.Product.Name}, Precio: {item.UnitPrice:C}, Cantidad: {item.Quantity}");
            }
        }
    }
}
