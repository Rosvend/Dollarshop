using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TiendaOnline
{
    public class Cliente
    {
        public static void VerProductosCreados()
        {
            Producto.VerTodosLosProductos();
        }

        public static void VerProductosEnCarrito()
        {
            foreach (var producto in Carrito.ListaProductosEnCarrito)
            {
                Console.WriteLine($"ID: {producto.IdProducto}, Nombre: {producto.NombreProducto}, Precio: {producto.Precio}");
            }
        }
    }
}
    
