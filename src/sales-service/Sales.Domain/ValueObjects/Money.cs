using Sales.Domain.Common;

namespace Sales.Domain.ValueObjects;

/// <summary>
/// A monetary amount in a specific <see cref="Currency"/>. Immutable: every
/// arithmetic operation yields a new instance. Invariant: <c>Amount &gt;= 0</c> —
/// Money never represents a debt in this domain. Operations across different
/// currencies are rejected.
/// </summary>
public sealed class Money : ValueObject
{
    public decimal Amount { get; }

    public Currency Currency { get; }

    public Money(decimal amount, Currency currency = Currency.USD)
    {
        if (amount < 0m)
        {
            throw new DomainException("A Money amount cannot be negative.");
        }

        if (!Enum.IsDefined(currency))
        {
            throw new DomainException("Money was given an unrecognized currency.");
        }

        Amount = amount;
        Currency = currency;
    }

    /// <summary>The zero amount in the given currency.</summary>
    public static Money Zero(Currency currency = Currency.USD) => new(0m, currency);

    public bool IsZero => Amount == 0m;

    /// <summary>Adds two amounts of the same currency.</summary>
    public Money Add(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount + other.Amount, Currency);
    }

    /// <summary>
    /// Subtracts another amount of the same currency. Throws if the result
    /// would be negative — protecting the "total never negative" invariant.
    /// </summary>
    public Money Subtract(Money other)
    {
        EnsureSameCurrency(other);
        var result = Amount - other.Amount;
        if (result < 0m)
        {
            throw new DomainException("A Money subtraction cannot produce a negative amount.");
        }

        return new Money(result, Currency);
    }

    /// <summary>Multiplies the amount by a positive quantity.</summary>
    public Money Multiply(Quantity quantity) => new(Amount * quantity.Value, Currency);

    /// <summary>Returns the fraction of this amount represented by <paramref name="percent"/>.</summary>
    public Money Percentage(decimal percent) => new(Amount * percent / 100m, Currency);

    /// <summary>True when this amount is strictly greater than <paramref name="other"/>.</summary>
    public bool IsGreaterThan(Money other)
    {
        EnsureSameCurrency(other);
        return Amount > other.Amount;
    }

    public static Money operator +(Money left, Money right) => left.Add(right);

    public static Money operator -(Money left, Money right) => left.Subtract(right);

    private void EnsureSameCurrency(Money other)
    {
        if (Currency != other.Currency)
        {
            throw new DomainException(
                $"Cannot operate on Money of different currencies ({Currency} and {other.Currency}).");
        }
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString() => $"{Amount:0.00} {Currency}";
}
