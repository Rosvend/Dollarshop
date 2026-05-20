namespace Catalog.Infrastructure.Persistence;

/// <summary>Persistence model for stock held during an in-flight checkout.</summary>
public sealed class CartStockReservation
{
    public Guid CartId { get; set; }

    public Guid ProductId { get; set; }

    public int Quantity { get; set; }
}
