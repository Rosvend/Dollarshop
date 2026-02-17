using TiendaOnline.Abstractions;
using TiendaOnline.Domain;

namespace TiendaOnline.Services
{
    public class CashPayment : IPaymentMethod
    {
        public string Name => "Efectivo";

        public bool Pay(decimal amount, PaymentInfo info)
        {
            return amount > 0;
        }
    }
}
