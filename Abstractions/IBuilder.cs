namespace TiendaOnline.Abstractions
{
    public interface IBuilder<out T>
    {
        T Build();
    }
}
