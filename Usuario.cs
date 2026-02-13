using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TiendaOnline
{
   

    public class Usuario
    {
        public string usuario { get; set; }
        public string Contraseña{ get; set; }

        public void MostrarInfo()
        {
            Console.WriteLine($"Usuario: {usuario}");
            Console.WriteLine($"Contraseña: {Contraseña}");


        }

        public static void IngresarUsuario()
        {
            bool credencialesCorrectas = false;

            while (!credencialesCorrectas)
            {
                Console.Write("Ingrese su Usuario: ");
                string usuario = Console.ReadLine();

                Console.Write("Ingrese su Contraseña: ");
                string Contraseña = Console.ReadLine();

                if (usuario.Equals("samuel") && Contraseña.Equals("123"))
                {
                    Usuario usuarioObj = new Usuario { usuario = usuario, Contraseña = Contraseña };
                    Console.WriteLine();
                    usuarioObj.MostrarInfo();
                    credencialesCorrectas = true; // Salir del bucle cuando las credenciales son correctas


                }
                else
                {
                    Console.WriteLine("\nUsuario o contraseña incorrectas. Por favor, inténtelo de nuevo. \n");
                }





              
        }


    }
}
    }
