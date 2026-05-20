namespace Sales.Domain.Common;

/// <summary>
/// Base class for Value Objects: immutable types compared by the value of their
/// components, not by identity. Subclasses declare those components through
/// <see cref="GetEqualityComponents"/>.
/// </summary>
public abstract class ValueObject : IEquatable<ValueObject>
{
    /// <summary>The ordered set of values that define equality for this object.</summary>
    protected abstract IEnumerable<object?> GetEqualityComponents();

    public bool Equals(ValueObject? other)
    {
        if (other is null || other.GetType() != GetType())
        {
            return false;
        }

        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    public override bool Equals(object? obj) => Equals(obj as ValueObject);

    public override int GetHashCode() =>
        GetEqualityComponents()
            .Aggregate(0, (hash, component) => HashCode.Combine(hash, component));

    public static bool operator ==(ValueObject? left, ValueObject? right) =>
        left is null ? right is null : left.Equals(right);

    public static bool operator !=(ValueObject? left, ValueObject? right) => !(left == right);
}
