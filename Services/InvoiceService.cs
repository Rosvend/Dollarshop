using TiendaOnline.Abstractions;
using TiendaOnline.Domain;

namespace TiendaOnline.Services
{
    public class InvoiceService : IInvoiceService
    {
        private int _nextId = 1;

        public Invoice Generate(ICart cart, int customerNumber, string paymentMethod)
        {
            var items = cart.GetItems();
            var total = cart.CalculateTotal();

            return new Invoice
            {
                Id = _nextId++,
                CustomerNumber = customerNumber,
                Items = items,
                Total = total,
                Date = DateTime.Now,
                PaymentMethod = paymentMethod
            };
        }
    }
}
