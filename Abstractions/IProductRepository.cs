using TiendaOnline.Domain;

namespace TiendaOnline.Abstractions
{
    public interface IProductRepository
    {
        List<Product> GetAll();
        Product? FindById(int id);
        void UpdateStock(int id, int qty);
    }
}
