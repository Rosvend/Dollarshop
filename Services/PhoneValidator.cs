using System.Text.RegularExpressions;
using TiendaOnline.Abstractions;

namespace TiendaOnline.Services
{
    public class PhoneValidator : IValidator<string>
    {
        public string ErrorMessage => "El telefono solo debe contener numeros.";

        public bool IsValid(string value)
        {
            return !string.IsNullOrWhiteSpace(value) && Regex.IsMatch(value, @"^\d+$");
        }
    }
}
