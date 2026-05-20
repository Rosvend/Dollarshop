using Sales.Domain.Common;

namespace Sales.Domain.ValueObjects;

/// <summary>
/// Strongly-typed identifier of a product. It originates in the Catalog context;
/// the Sales context only references it as an opaque, validated value.
/// </summary>
public sealed class ProductId : ValueObject
{
    public Guid Value { get; }

    public ProductId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new DomainException("A ProductId cannot be empty.");
        }

        Value = value;
    }

    /// <summary>Creates a brand-new unique identifier.</summary>
    public static ProductId New() => new(Guid.NewGuid());

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();
}
