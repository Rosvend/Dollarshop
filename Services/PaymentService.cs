using TiendaOnline.Abstractions;
using TiendaOnline.Domain;

namespace TiendaOnline.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly List<IPaymentMethod> _methods;

        public PaymentService(IEnumerable<IPaymentMethod> methods)
        {
            _methods = methods.ToList();
        }

        public List<string> GetAvailableMethods()
        {
            return _methods.Select(m => m.Name).ToList();
        }

        public bool Process(string methodName, decimal amount, PaymentInfo info)
        {
            var method = _methods.FirstOrDefault(
                m => m.Name.Equals(methodName, StringComparison.OrdinalIgnoreCase));

            if (method == null)
            {
                return false;
            }

            return method.Pay(amount, info);
        }
    }
}
