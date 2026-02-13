using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TiendaOnline
{
    using System;
    using System.Text.RegularExpressions;

    public class Registro
    {
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Email { get; set; }
        public string Telefono { get; set; }

        public void MostrarInfo()
        {
            Console.WriteLine($"Nombre: {Nombre}");
            Console.WriteLine($"Apellido: {Apellido}");
            Console.WriteLine($"Email: {Email}");
            Console.WriteLine($"Teléfono: {Telefono}");
        }

        public static void RegistrarUsuario()
        {
            string nombre;
            string apellido;
            string email;
            string telefono;

            while (true)
            {
                Console.Write("Ingrese su nombre: ");
                nombre = Console.ReadLine();

                if (EsValidoNombreOApellido(nombre))
                {
                    break; // Salir del bucle si el nombre es válido.
                }

                Console.WriteLine("El nombre solo debe contener letras. Intente nuevamente.");
            }

            while (true)
            {
                Console.Write("Ingrese su apellido: ");
                apellido = Console.ReadLine();

                if (EsValidoNombreOApellido(apellido))
                {
                    break; // Salir del bucle si el apellido es válido.
                }

                Console.WriteLine("El apellido solo debe contener letras. Intente nuevamente.");
            }

            while (true)
            {
                Console.Write("Ingrese su email: ");
                email = Console.ReadLine();

                if (EsEmailValido(email))
                {
                    break; // Salir del bucle si el email es válido.
                }

                Console.WriteLine("El email debe contener el carácter '@'. Intente nuevamente.");
            }

            while (true)
            {
                Console.Write("Ingrese su teléfono: ");
                telefono = Console.ReadLine();

                if (EsTelefonoValido(telefono))
                {
                    break; // Salir del bucle si el teléfono es válido.
                }

                Console.WriteLine("El teléfono solo debe contener números. Intente nuevamente.");
            }

            Registro registroObj = new Registro { Nombre = nombre, Apellido = apellido, Email = email, Telefono = telefono };
            Console.WriteLine();
            registroObj.MostrarInfo();
        }

        private static bool EsValidoNombreOApellido(string texto)
        {
            // Utilizamos una expresión regular para verificar si el texto contiene solo letras.
            // La expresión regular ^[a-zA-Z]+$ significa que debe ser una secuencia de una o más letras (mayúsculas o minúsculas).
            return Regex.IsMatch(texto, "^[a-zA-Z]+$");
        }

        private static bool EsEmailValido(string email)
        {
            // Utilizamos una expresión regular para verificar que el email contenga el carácter '@'.
            return Regex.IsMatch(email, @"@");
        }

        private static bool EsTelefonoValido(string telefono)
        {
            // Utilizamos una expresión regular para verificar que el teléfono contenga solo números.
            return Regex.IsMatch(telefono, "^[0-9]+$");
        }
    }
}