using System;
using TiendaOnline;

namespace TiendaOnline
{

    class Program
    {
        static void Main()
        {


            // Creacion de algunos productos
            Producto.AgregarProducto(1, "Portatil", 1000, 1);
            Producto.AgregarProducto(2, "Celular", 550, 1);
            Producto.AgregarProducto(3, "Airpods", 80, 1);
            Producto.AgregarProducto(4, "tarjeta grafica", 800, 1);
            Producto.AgregarProducto(5, "Mouse", 22.9, 1);
            Producto.AgregarProducto(6, "Parlantes", 300, 1);
            Producto.AgregarProducto(7, "Bose", 400, 1);
            
            int opcion, opcion1, opcion2, opcion3,opcion4,opcion5,opcion6;


                
            {
                Console.WriteLine("-----BIENVENIDO-----\n");
                Console.WriteLine("1.Iniciar Sesion");
                Console.WriteLine("2.Registrarse \n");
                Console.Write("Seleccione una opción: ");

                if (int.TryParse(Console.ReadLine(), out opcion))
                {
                    switch (opcion)
                    {
                        case 1:
                            Console.WriteLine();
                            Usuario.IngresarUsuario();
                            Console.WriteLine("\nINICIO EXITOSO\n");
                            break;
                        case 2:
                            Console.WriteLine();
                            Registro.RegistrarUsuario();
                            Console.WriteLine("\nREGISTRO EXITOSO\n");
                            break;
                        default:
                            Console.WriteLine("Opción no válida. Por favor, vuelva a intentar.\n");
                            break;
                    }
                }
                else
                {
                    Console.WriteLine("Por favor ingrese solo números.\n");
                }
            } while (opcion != 1 && opcion != 2);


            do
            {
                Console.WriteLine("-----MENU PRINCIPAL-----\n");

                Console.WriteLine("1.CLIENTE");
                Console.WriteLine("2.PRODUCTOS");
                Console.WriteLine("3.CARRITO");
                Console.WriteLine("0.SALIR \n");
                Console.WriteLine("Ingrese una opcion: ");
                opcion1 = int.Parse(Console.ReadLine());
                Console.WriteLine();

                switch (opcion1)
                {

                    case 1:

                        do

                        {

                            Console.WriteLine("CLIENTE");
                            Console.WriteLine("1.VerProductos");
                            Console.WriteLine("2.VerCarrito");
                            Console.WriteLine("0.Volver");
                            Console.WriteLine("Elija una opcion: ");
                            opcion2 = Convert.ToInt32(Console.ReadLine());
                            Console.WriteLine();

                            switch (opcion2)
                            {

                                case 1:
                                    Console.WriteLine();
                                    Console.WriteLine("LISTA DE PRODUCTOS:");
                                    Producto.VerTodosLosProductos();
                                    Console.WriteLine();

                                    break;

                                case 2:
                                    Console.WriteLine();
                                    Console.WriteLine("PRODUCTOS EN EL CARRITO:");
                                    Cliente.VerProductosEnCarrito();
                                    Console.WriteLine();

                                    break;

                                default:

                                    Console.WriteLine("INGRESE UNA OPCION VALIDA");

                                    break;

                            }


                        } while (opcion2 != 0);

                        break;

                    case 2:

                        do
                        {

                            Console.WriteLine("PRODUCTO");
                            Console.WriteLine("1.Agregar al carrito");
                            Console.WriteLine("0.volver ");
                            Console.WriteLine("Elija una opcion: ");
                            opcion3 = Convert.ToInt32(Console.ReadLine());
                            Console.WriteLine();


                            switch (opcion3)
                            {

                                case 1:

                                    Producto.AgregarProductoCarrito();

                                    break;

                                default:

                                    Console.WriteLine("Ingrese un valor valido");

                                    break;

                            }


                        } while (opcion3 != 0);


                        break;

                        case 3:

                        do {

                            Console.WriteLine("-----CARRITO-----");
                            Console.WriteLine();
                            Console.WriteLine("PRODUCTOS EN EL CARRITO:");
                            Cliente.VerProductosEnCarrito();
                            Console.WriteLine();
                            Console.WriteLine("1.Sacar producto");
                            Console.WriteLine("2.Calular total");
                            Console.WriteLine("3.Hacer pago");
                            Console.WriteLine("0.Volver \n");
                            Console.WriteLine("Elija una opcion: ");
                            opcion4 = Convert.ToInt32(Console.ReadLine());
                            Console.WriteLine();

                            switch (opcion4)
                            {

                                case 1:

                                    Carrito.SacarProductoDelCarritoPorConsola();

                                    break;

                                    case 2:

                                    double total = Carrito.CalcularTotal();
                                    Console.WriteLine($"Total del Carrito: {total} \n");
                                    Console.WriteLine();
                                    

                                    break;

                                    case 3:


                                    Console.WriteLine("-----PAGAR-----");

                                    Pagar pagar = new Pagar();  
                                    Pagar.IngresarMetodoPago();
                                    Pagar.IngresarNumeroCuenta();
                                    Console.WriteLine();
                                    Console.WriteLine("ACEPTADO");
                                    Console.WriteLine();

                                    Console.WriteLine("1.Confirmar pago ");
                                    Console.WriteLine("2.Volver \n ");
                                    Console.WriteLine();
                                    Console.WriteLine("Elija una opcion : ");
                                    opcion5 = Convert.ToInt32(Console.ReadLine());  

                                    switch (opcion5) 
                                    {

                                        case 1:

                                            Console.WriteLine("-----FACTURA-----");

                                            Factura factura = new Factura();
                                            Console.WriteLine();
                                            
                                            Console.WriteLine();

                                            Console.WriteLine("1.Generar recibo");
                                            Console.WriteLine("0.volver \n");
                                            Console.WriteLine("Elija una opcion");
                                            opcion6 = Convert.ToInt32(Console.ReadLine());



                                            switch (opcion6)
                                            {

                                                case 1:
                                                    Console.WriteLine("\nIdentificacion: " + Factura.Identificacion() + "\nTotal a pagar: " + Carrito.CalcularTotal() + "\nFecha: " + Factura.Fecha());
                                                    Console.WriteLine();
                                                    Console.WriteLine();
                                                    Console.WriteLine();
                                                    Console.WriteLine("-----SU RECIBO A SIDO GENERADO-----");




                                                    


                                                    Console.WriteLine("\n\n\n\n\n");
                                                    break;

                                                default:

                                                    break;

                                                   



                                            }


                                            





                                            break;

                                        default:



                                            Console.WriteLine("ingrese una opcion valida");

                                            break;





                                    }

                                    









                                            break;

                                    default:

                                       Console.WriteLine("Ingrese una opcion valida");

                                    break;

                            }


                        } while(opcion4 != 0);

                        break;



                    default:

                        Console.WriteLine("Ingrese una opcion valida");

                        break;



                } // switch opcion1



            } while (opcion1 != 0);







        }

    }


}











