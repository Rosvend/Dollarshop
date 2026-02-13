using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TiendaOnline
{
    public class Factura
    {

        public static int Identificacion()
        {
            int identificacion;
            bool entradaValida = false;

            do
            {
                Console.WriteLine("Ingrese su identificacion: ");
                string input = Console.ReadLine();

                if (int.TryParse(input, out identificacion))
                {
                    entradaValida = true;
                }
                else
                {
                    Console.WriteLine("Por favor, ingrese solo números.");
                }

            } while (!entradaValida);

            return identificacion;
        }
        public static void Monto()
        {
            
            double total = Carrito.CalcularTotal();
            Console.WriteLine($"Total del a pagar: {total} ");
            


        }

        public static DateTime Fecha()
        {
            DateTime fecha;
            bool entradaValida = false;

            do
            {
                Console.WriteLine("Ingrese la fecha en formato dd-mm-yy: ");
                string input = Console.ReadLine();

                if (DateTime.TryParseExact(input, "dd-MM-yy", CultureInfo.InvariantCulture, DateTimeStyles.None, out fecha))
                {
                    entradaValida = true;
                }
                else
                {
                    Console.WriteLine("Por favor, ingrese la fecha en el formato correcto (dd-mm-yy).");
                }

            } while (!entradaValida);

            return fecha;
        }

     }
}
