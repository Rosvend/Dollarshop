namespace TiendaOnline.Abstractions
{
    public interface IDiscountStrategy
    {
        decimal Apply(decimal price);
    }
}
