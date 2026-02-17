using TiendaOnline.Abstractions;
using TiendaOnline.Domain;

namespace TiendaOnline.Services
{
    public class InMemoryProductRepository : IProductRepository
    {
        private readonly List<Product> _products = new();

        public void Seed(IEnumerable<Product> products)
        {
            _products.AddRange(products);
        }

        public List<Product> GetAll()
        {
            return _products.ToList();
        }

        public Product? FindById(int id)
        {
            return _products.FirstOrDefault(p => p.Id == id);
        }

        public void UpdateStock(int id, int qty)
        {
            var product = FindById(id);
            if (product != null)
            {
                product.StockQuantity += qty;
            }
        }
    }
}
