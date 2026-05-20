using Identity.Domain.Common;

namespace Identity.Domain.ValueObjects;

public sealed class UserId : ValueObject
{
    public Guid Value { get; }

    public UserId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new DomainException("A UserId cannot be empty.");
        }

        Value = value;
    }

    public static UserId New() => new(Guid.NewGuid());

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
