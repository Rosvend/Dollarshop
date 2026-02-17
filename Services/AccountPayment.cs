using TiendaOnline.Abstractions;
using TiendaOnline.Domain;

namespace TiendaOnline.Services
{
    public class AccountPayment : IPaymentMethod
    {
        public string Name => "Cuenta";

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
