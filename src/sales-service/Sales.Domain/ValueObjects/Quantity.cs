using Sales.Domain.Common;

namespace Sales.Domain.ValueObjects;

/// <summary>
/// A positive count of units of a product. Invariant: <c>Value &gt;= 1</c> —
/// zero or negative quantities are never added to a cart.
/// </summary>
public sealed class Quantity : ValueObject
{
    public int Value { get; }

    public Quantity(int value)
    {
        if (value < 1)
        {
            throw new DomainException("A Quantity must be at least 1.");
        }

        Value = value;
    }

    /// <summary>Returns a new quantity increased by <paramref name="by"/>.</summary>
    public Quantity Increase(Quantity by) => new(Value + by.Value);

    /// <summary>
    /// Returns a new quantity decreased by <paramref name="by"/>.
    /// Throws if the result would fall below 1.
    /// </summary>
    public Quantity Decrease(Quantity by)
    {
        var result = Value - by.Value;
        if (result < 1)
        {
            throw new DomainException("A Quantity cannot be decreased below 1.");
        }

        return new Quantity(result);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();
}
