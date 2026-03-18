using TiendaOnline.Abstractions;
using TiendaOnline.Domain;

namespace TiendaOnline.Services
{
    public class InvoiceService : IInvoiceService
    {
        private int _nextId = 1;
        private readonly Func<IInvoiceBuilder> _builderFactory;

        public InvoiceService(Func<IInvoiceBuilder> builderFactory)
        {
            _builderFactory = builderFactory;
        }

        public Invoice Generate(ICart cart, int customerNumber, string paymentMethod)
        {
            // El servicio orquesta la secuencia de construcción (actúa como Director)
            return _builderFactory()
                .WithId(_nextId++)
                .WithCustomerNumber(customerNumber)
                .WithItems(cart.GetItems())
                .WithTotal(cart.CalculateTotal())
                .WithDate(DateTime.Now)
                .WithPaymentMethod(paymentMethod)
                .Build();
        }
    }
}
