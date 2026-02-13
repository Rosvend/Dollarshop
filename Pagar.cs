using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TiendaOnline
{
    public class Pagar
    {
        public static void IngresarMetodoPago()
        {
            bool metodoValido = false;

            while (!metodoValido)
            {
                Console.WriteLine("Ingrese el metodo de pago (Tarjeta, Efectivo, Cuenta): ");
                String Metodo_pago = Console.ReadLine();

                if (Metodo_pago.Equals("Tarjeta", StringComparison.OrdinalIgnoreCase) ||
                    Metodo_pago.Equals("Efectivo", StringComparison.OrdinalIgnoreCase) ||
                    Metodo_pago.Equals("Cuenta", StringComparison.OrdinalIgnoreCase))
                {
                    metodoValido = true; // Salir del bucle cuando se proporciona un método válido
                }
                else
                {
                    Console.WriteLine("\nMETODO DE PAGO INVALIDO\n");
                    Console.WriteLine("INTENTÉLO NUEVAMENTE \n");
                }
            }
        }

        public static double IngresarNumeroCuenta()
        {
            double numeroCuenta;
            bool entradaValida = false;

            do
            {
                Console.WriteLine("Ingrese numero de cuenta: ");
                string N_cuenta = Console.ReadLine();

                if (double.TryParse(N_cuenta, out numeroCuenta))
                {
                    entradaValida = true;
                }
                else
                {
                    Console.WriteLine("Por favor, ingrese solo números.");
                }

            } while (!entradaValida);

            return numeroCuenta;
        }
    }
}
