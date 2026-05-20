using Sales.Domain.Common;

namespace Sales.Domain.ValueObjects;

/// <summary>
/// An immutable snapshot of a Catalog product, taken at the moment it is added
/// to a cart. The cart never holds a live reference to the Catalog: a later
/// price change there does not mutate carts already holding this reference.
/// This is the implicit Anti-Corruption Layer protecting the Sales core.
/// </summary>
public sealed class ProductReference : ValueObject
{
    public ProductId ProductId { get; }

    public string SnapshotName { get; }

    public Money SnapshotPrice { get; }

    public ProductReference(ProductId productId, string snapshotName, Money snapshotPrice)
    {
        if (productId is null)
        {
            throw new DomainException("A ProductReference requires a ProductId.");
        }

        if (string.IsNullOrWhiteSpace(snapshotName))
        {
            throw new DomainException("A ProductReference requires a non-empty product name.");
        }

        if (snapshotPrice is null)
        {
            throw new DomainException("A ProductReference requires a snapshot price.");
        }

        ProductId = productId;
        SnapshotName = snapshotName;
        SnapshotPrice = snapshotPrice;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return ProductId;
        yield return SnapshotName;
        yield return SnapshotPrice;
    }
}
