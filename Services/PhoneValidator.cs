namespace TiendaOnline.Services
{
    public class PhoneValidator : RegexValidator
    {
        public override string ErrorMessage => "El telefono solo debe contener numeros.";
        protected override string Pattern => @"^\d+$";
    }
}
