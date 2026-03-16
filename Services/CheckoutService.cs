using TiendaOnline.Abstractions;
using TiendaOnline.Domain;

namespace TiendaOnline.Services
{
    public class CheckoutService : ICheckoutService
    {
        private readonly PaymentService _paymentService;
        private readonly IInvoiceService _invoiceService;

        public CheckoutService(PaymentService paymentService, IInvoiceService invoiceService)
        {
            _paymentService = paymentService;
            _invoiceService = invoiceService;
        }

        public List<string> GetAvailablePaymentMethods() => _paymentService.GetAvailableMethods();

        public CheckoutResult Process(ICart cart, PaymentInfo paymentInfo, int customerNumber)
        {
            bool paid = _paymentService.Process(paymentInfo.Method, cart.CalculateTotal(), paymentInfo);
            if (!paid) return new CheckoutResult { Success = false };

            var invoice = _invoiceService.Generate(cart, customerNumber, paymentInfo.Method);
            return new CheckoutResult { Success = true, Invoice = invoice };
        }
    }
}
