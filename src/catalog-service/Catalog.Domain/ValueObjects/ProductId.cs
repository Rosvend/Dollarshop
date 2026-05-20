using Catalog.Domain.Common;

namespace Catalog.Domain.ValueObjects;

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

    public static ProductId New() => new(Guid.NewGuid());

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();
}
