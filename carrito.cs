using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TiendaOnline
{
    public class Carrito
    {
        public static List<Producto> ListaProductosEnCarrito = new List<Producto>();

        public static void AgregarProductoAlCarrito(Producto producto)
        {
            ListaProductosEnCarrito.Add(producto);
        }
        public static double CalcularTotal()
        {
            double total = 0;
            foreach (var producto in ListaProductosEnCarrito)
            {
                total += producto.Precio;
            }
            return total;
        }

        public static void SacarProductoDelCarritoPorConsola()
        {
            Console.WriteLine("Ingrese el ID del producto que desea sacar del carrito (ingrese 0 para cancelar):");
            int idProducto;
            while (true)
            {
                if (int.TryParse(Console.ReadLine(), out idProducto))
                {
                    if (idProducto == 0)
                        break;

                    Producto producto = ListaProductosEnCarrito.FirstOrDefault(p => p.IdProducto == idProducto);

                    if (producto != null)
                    {
                        ListaProductosEnCarrito.Remove(producto);
                        producto.CantidadDisponible++;
                        Console.WriteLine($"Producto con ID {idProducto} sacado del carrito.");
                    }
                    else
                    {
                        Console.WriteLine("Producto no encontrado en el carrito.");
                    }
                }
                else
                {
                    Console.WriteLine("Entrada inválida. Por favor, ingrese un número válido");
                }
            }
        }
    }
}



    