using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TiendaOnline
{
    public class Ofertas
    {
        public double Descuento { get; set; }

        public Ofertas(double descuento)
        {
            Descuento = descuento;
        }

        public static void AplicarDescuentoAProducto(Producto producto, double descuento)
        {
            producto.Precio -= (producto.Precio * (descuento / 100));
        }

        public static void AgregarProductoConDescuentoAlCarrito(int id, double descuento)
        {
            Producto producto = Producto.ListaProductos.Find(p => p.IdProducto == id);

            if (producto != null && producto.CantidadDisponible > 0)
            {
                AplicarDescuentoAProducto(producto, descuento);
                Carrito.AgregarProductoAlCarrito(producto);
                producto.CantidadDisponible--;
            }
            else
            {
                Console.WriteLine("Producto no disponible.");
            }
        }

    }
}
