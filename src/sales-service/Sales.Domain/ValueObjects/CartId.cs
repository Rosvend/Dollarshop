using Sales.Domain.Common;

namespace Sales.Domain.ValueObjects;

/// <summary>Strongly-typed identifier of a <c>ShoppingCart</c> aggregate.</summary>
public sealed class CartId : ValueObject
{
    public Guid Value { get; }

    public CartId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new DomainException("A CartId cannot be empty.");
        }

        Value = value;
    }

    /// <summary>Creates a brand-new unique identifier.</summary>
    public static CartId New() => new(Guid.NewGuid());

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();
}
