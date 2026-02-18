namespace TiendaOnline.Abstractions
{
    public interface IValidator<T>
    {
        bool IsValid(T value);
        string ErrorMessage { get; }
    }
}
