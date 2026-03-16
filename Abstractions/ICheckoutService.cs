using TiendaOnline.Domain;

namespace TiendaOnline.Abstractions
{
    public interface ICheckoutService
    {
        List<string> GetAvailablePaymentMethods();
        CheckoutResult Process(ICart cart, PaymentInfo paymentInfo, int customerNumber);
    }
}
