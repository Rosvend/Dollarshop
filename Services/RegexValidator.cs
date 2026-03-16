using System.Text.RegularExpressions;
using TiendaOnline.Abstractions;

namespace TiendaOnline.Services
{
    public abstract class RegexValidator : IValidator<string>
    {
        public abstract string ErrorMessage { get; }
        protected abstract string Pattern { get; }

        public bool IsValid(string value)
        {
            return !string.IsNullOrWhiteSpace(value) && Regex.IsMatch(value, Pattern);
        }
    }
}
