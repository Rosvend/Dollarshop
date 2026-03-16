namespace TiendaOnline.Services
{
    public class EmailValidator : RegexValidator
    {
        public override string ErrorMessage => "El email debe tener un formato valido (ejemplo@dominio.com).";
        protected override string Pattern => @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
    }
}
