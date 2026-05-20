using Catalog.Domain.Common;

namespace Catalog.Domain.ValueObjects;

/// <summary>Available units on hand for a published product.</summary>
public sealed class StockLevel : ValueObject
{
    public int OnHand { get; }

    public StockLevel(int onHand)
    {
        if (onHand < 0)
        {
            throw new DomainException("Stock on hand cannot be negative.");
        }

        OnHand = onHand;
    }

    public static StockLevel Empty() => new(0);

    public StockLevel Reserve(int quantity)
    {
        if (quantity < 1)
        {
            throw new DomainException("A stock reservation must be at least 1 unit.");
        }

        if (OnHand < quantity)
        {
            throw new DomainException($"Insufficient stock: requested {quantity}, available {OnHand}.");
        }

        return new StockLevel(OnHand - quantity);
    }

    public StockLevel Release(int quantity)
    {
        if (quantity < 1)
        {
            throw new DomainException("A stock release must be at least 1 unit.");
        }

        return new StockLevel(OnHand + quantity);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return OnHand;
    }
}
