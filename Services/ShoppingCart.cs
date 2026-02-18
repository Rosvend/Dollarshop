using TiendaOnline.Abstractions;
using TiendaOnline.Domain;

namespace TiendaOnline.Services
{
    public class ShoppingCart : ICart
    {
        private readonly List<CartItem> _items = new();
        private readonly IDiscountStrategy? _discountStrategy;

        public ShoppingCart(IDiscountStrategy? discountStrategy = null)
        {
            _discountStrategy = discountStrategy;
        }

        public void AddItem(Product product, int qty)
        {
            var existing = _items.FirstOrDefault(i => i.Product.Id == product.Id);
            if (existing != null)
            {
                existing.Quantity += qty;
            }
            else
            {
                _items.Add(new CartItem
                {
                    Product = product,
                    Quantity = qty,
                    UnitPrice = product.Price
                });
            }
        }

        public void RemoveItem(int productId)
        {
            var item = _items.FirstOrDefault(i => i.Product.Id == productId);
            if (item != null)
            {
                _items.Remove(item);
            }
        }

        public List<CartItem> GetItems()
        {
            return _items.ToList();
        }

        public decimal CalculateTotal()
        {
            decimal total = _items.Sum(i => i.UnitPrice * i.Quantity);

            if (_discountStrategy != null)
            {
                total = _discountStrategy.Apply(total);
            }

            return total;
        }
    }
}
