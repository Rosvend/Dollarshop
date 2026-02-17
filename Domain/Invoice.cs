namespace TiendaOnline.Domain
{
    public class Invoice
    {
        public int Id { get; set; }
        public int CustomerNumber { get; set; }
        public List<CartItem> Items { get; set; } = new List<CartItem>();
        public decimal Total { get; set; }
        public DateTime Date { get; set; }
        public string PaymentMethod { get; set; }
    }
}
