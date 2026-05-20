using Sales.Domain.Common;

namespace Sales.Domain.ValueObjects;

/// <summary>
/// Strongly-typed identifier of a <c>CartItem</c>. The identity is meaningful
/// only inside the <c>ShoppingCart</c> aggregate that owns the item.
/// </summary>
public sealed class CartItemId : ValueObject
{
    public Guid Value { get; }

    public CartItemId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new DomainException("A CartItemId cannot be empty.");
        }

        Value = value;
    }

    /// <summary>Creates a brand-new unique identifier.</summary>
    public static CartItemId New() => new(Guid.NewGuid());

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();
}
