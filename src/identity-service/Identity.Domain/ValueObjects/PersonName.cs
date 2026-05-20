using Identity.Domain.Common;

namespace Identity.Domain.ValueObjects;

public sealed class PersonName : ValueObject
{
    public string Value { get; }

    public PersonName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("A person name cannot be empty.");
        }

        if (value.Trim().Length < 2)
        {
            throw new DomainException("A person name must be at least 2 characters.");
        }

        Value = value.Trim();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
