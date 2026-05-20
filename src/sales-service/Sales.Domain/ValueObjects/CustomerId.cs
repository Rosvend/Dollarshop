using Sales.Domain.Common;

namespace Sales.Domain.ValueObjects;

/// <summary>
/// Strongly-typed identifier of the customer who owns a cart. It originates in
/// the Identity context and crosses into Sales as a conformist reference.
/// </summary>
public sealed class CustomerId : ValueObject
{
    public Guid Value { get; }

    public CustomerId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new DomainException("A CustomerId cannot be empty.");
        }

        Value = value;
    }

    /// <summary>Creates a brand-new unique identifier.</summary>
    public static CustomerId New() => new(Guid.NewGuid());

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();
}
