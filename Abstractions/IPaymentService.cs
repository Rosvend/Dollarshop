using TiendaOnline.Domain;

namespace TiendaOnline.Abstractions
{
    public interface IPaymentService
    {
        List<string> GetAvailableMethods();
        bool Process(string methodName, decimal amount, PaymentInfo info);
    }
}
