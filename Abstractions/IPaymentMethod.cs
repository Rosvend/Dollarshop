using TiendaOnline.Domain;

namespace TiendaOnline.Abstractions
{
    public interface IPaymentMethod
    {
        bool Pay(decimal amount, PaymentInfo info);
        string Name { get; }
    }
}
