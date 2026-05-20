using Catalog.Domain.Common;
using Catalog.Domain.ValueObjects;

namespace Catalog.Domain.Aggregates;

/// <summary>
/// Aggregate root of the Catalog bounded context. Owns list price and stock;
/// <see cref="Reserve"/> and <see cref="Release"/> guard inventory invariants.
/// </summary>
public sealed class Product : AggregateRoot<ProductId>
{
    public Sku Sku { get; }

    public ProductName Name { get; }

    public Money ListPrice { get; }

    public StockLevel Stock { get; private set; }

    public Product(
        ProductId id,
        Sku sku,
        ProductName name,
        Money listPrice,
        StockLevel stock) : base(id)
    {
        Sku = sku ?? throw new DomainException("A Product requires a Sku.");
        Name = name ?? throw new DomainException("A Product requires a name.");
        ListPrice = listPrice ?? throw new DomainException("A Product requires a list price.");
        Stock = stock ?? throw new DomainException("A Product requires a stock level.");
    }

    public void Reserve(int quantity) => Stock = Stock.Reserve(quantity);

    public void Release(int quantity) => Stock = Stock.Release(quantity);
}
