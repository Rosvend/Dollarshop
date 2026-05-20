using Catalog.Domain.Common;

namespace Catalog.Domain.ValueObjects;

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

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString() => $"{Amount:0.00} {Currency}";
}
