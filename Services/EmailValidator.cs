using System.Text.RegularExpressions;
using TiendaOnline.Abstractions;

namespace TiendaOnline.Services
{
    public class EmailValidator : IValidator<string>
    {
        public string ErrorMessage => "El email debe tener un formato valido (ejemplo@dominio.com).";

        public bool IsValid(string value)
        {
            return !string.IsNullOrWhiteSpace(value) && Regex.IsMatch(value, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }
    }
}
