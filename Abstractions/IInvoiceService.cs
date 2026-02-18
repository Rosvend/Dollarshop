using TiendaOnline.Domain;

namespace TiendaOnline.Abstractions
{
    public interface IInvoiceService
    {
        Invoice Generate(ICart cart, int customerNumber, string paymentMethod);
    }
}
