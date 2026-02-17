using TiendaOnline.Domain;

namespace TiendaOnline.Abstractions
{
    public interface ICart
    {
        void AddItem(Product product, int qty);
        void RemoveItem(int productId);
        List<CartItem> GetItems();
        decimal CalculateTotal();
    }
}
