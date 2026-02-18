using System.Text.RegularExpressions;
using TiendaOnline.Abstractions;

namespace TiendaOnline.Services
{
    public class NameValidator : IValidator<string>
    {
        public string ErrorMessage => "El nombre solo debe contener letras.";

        public bool IsValid(string value)
        {
            return !string.IsNullOrWhiteSpace(value) && Regex.IsMatch(value, "^[a-zA-ZáéíóúÁÉÍÓÚñÑ]+$");
        }
    }
}
