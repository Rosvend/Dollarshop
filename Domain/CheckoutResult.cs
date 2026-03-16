namespace TiendaOnline.Domain
{
    public class CheckoutResult
    {
        public bool Success { get; set; }
        public Invoice? Invoice { get; set; }
    }
}
