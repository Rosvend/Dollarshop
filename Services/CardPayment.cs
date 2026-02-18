using TiendaOnline.Abstractions;
using TiendaOnline.Domain;

namespace TiendaOnline.Services
{
    public class CardPayment : IPaymentMethod
    {
        public string Name => "Tarjeta";

        public bool Pay(decimal amount, PaymentInfo info)
        {
            if (string.IsNullOrWhiteSpace(info.AccountNumber))
            {
                return false;
            }

            return amount > 0;
        }
    }
}
