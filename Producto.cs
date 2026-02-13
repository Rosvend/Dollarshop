using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TiendaOnline;

namespace TiendaOnline
{
    public class Producto
    
      
        {
            public int IdProducto { get; set; }
            public string NombreProducto { get; set; }
            public double Precio { get; set; }
            public int CantidadDisponible { get; set; }

            public static List<Producto> ListaProductos = new List<Producto>();

            public static void AgregarProducto(int id, string nombre, double precio, int cantidad)
            {
                Producto producto = new Producto
                {


                    IdProducto = id,
                    NombreProducto = nombre,
                    Precio = precio,
                    CantidadDisponible = cantidad
                };
                ListaProductos.Add(producto);
            }

            public static void VerTodosLosProductos()
            {
                foreach (var producto in ListaProductos)
                {
                    Console.WriteLine($"ID: {producto.IdProducto}, Nombre: {producto.NombreProducto}, Precio: {producto.Precio}, Cantidad Disponible: {producto.CantidadDisponible}");
                }
            }

        // saber la dispónibilidad
            public static void AgregarAlCarrito(int id)
            {
                Producto producto = ListaProductos.Find(p => p.IdProducto == id);
                if (producto != null && producto.CantidadDisponible > 0)
                {
                    Carrito.AgregarProductoAlCarrito(producto);
                    producto.CantidadDisponible--;
                }
                else
                {
                    Console.WriteLine("Producto no disponible, agregue otro producto.");
                }


            }
        public static void AgregarProductoCarrito()
        {
            Console.WriteLine("Ingrese el ID del producto que desea agregar al carrito (ingrese 0 para salir) :");
            int idProducto;
            while (true)
            {
                if (int.TryParse(Console.ReadLine(), out idProducto))
                {
                    if (idProducto == 0)
                        break;

                    Producto.AgregarAlCarrito(idProducto);
                    Console.WriteLine($"Producto con ID {idProducto} agregado al carrito.");
                }
                else
                {
                    Console.WriteLine("Entrada inválida. Por favor, ingrese un número válido o 0 para salir.");
                }
            }
        }




        }

}



