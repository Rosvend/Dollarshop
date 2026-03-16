namespace TiendaOnline.Services
{
    public class NameValidator : RegexValidator
    {
        public override string ErrorMessage => "El nombre solo debe contener letras.";
        protected override string Pattern => "^[a-zA-ZáéíóúÁÉÍÓÚñÑ]+$";
    }
}
