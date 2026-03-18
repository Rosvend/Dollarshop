using TiendaOnline.Abstractions;

namespace TiendaOnline.Domain.Builders
{
    public class InvoiceBuilder : IInvoiceBuilder
    {
        private int _id = 0;
        private int _customerNumber = 0;
        private List<CartItem> _items = new();
        private decimal _total = 0;
        private DateTime _date = DateTime.Now;
        private string _paymentMethod = string.Empty;

        public IInvoiceBuilder WithId(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Id debe ser mayor a 0");
            _id = id;
            return this;
        }

        public IInvoiceBuilder WithCustomerNumber(int customerNumber)
        {
            if (customerNumber <= 0)
                throw new ArgumentException("Customer Number debe ser mayor a 0");
            _customerNumber = customerNumber;
            return this;
        }

        public IInvoiceBuilder WithItems(List<CartItem> items)
        {
            if (items == null || items.Count == 0)
                throw new ArgumentException("Items no puede estar vacío");
            _items = items;
            return this;
        }

        public IInvoiceBuilder WithTotal(decimal total)
        {
            if (total < 0)
                throw new ArgumentException("Total no puede ser negativo");
            _total = total;
            return this;
        }

        public IInvoiceBuilder WithDate(DateTime date)
        {
            _date = date;
            return this;
        }

        public IInvoiceBuilder WithPaymentMethod(string paymentMethod)
        {
            if (string.IsNullOrWhiteSpace(paymentMethod))
                throw new ArgumentException("PaymentMethod es requerido");
            _paymentMethod = paymentMethod;
            return this;
        }

        public Invoice Build()
        {
            ValidateRequired(_id > 0, "Id es requerido");
            ValidateRequired(_customerNumber > 0, "CustomerNumber es requerido");
            ValidateRequired(_items.Count > 0, "Items es requerido");
            ValidateRequired(_total >= 0, "Total es requerido");
            ValidateRequired(!string.IsNullOrWhiteSpace(_paymentMethod), "PaymentMethod es requerido");

            return new Invoice
            {
                Id = _id,
                CustomerNumber = _customerNumber,
                Items = _items,
                Total = _total,
                Date = _date,
                PaymentMethod = _paymentMethod
            };
        }

        private void ValidateRequired(bool condition, string message)
        {
            if (!condition)
                throw new ArgumentException(message);
        }
    }
}
