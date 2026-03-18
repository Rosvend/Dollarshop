using TiendaOnline.Domain;

namespace TiendaOnline.Abstractions
{
    public interface IInvoiceBuilder : IBuilder<Invoice>
    {
        IInvoiceBuilder WithId(int id);
        IInvoiceBuilder WithCustomerNumber(int customerNumber);
        IInvoiceBuilder WithItems(List<CartItem> items);
        IInvoiceBuilder WithTotal(decimal total);
        IInvoiceBuilder WithDate(DateTime date);
        IInvoiceBuilder WithPaymentMethod(string paymentMethod);
    }
}
