using Identity.Domain.Common;

namespace Identity.Domain.ValueObjects;

/// <summary>Stored credential — never the plaintext password.</summary>
public sealed class PasswordHash : ValueObject
{
    public string Value { get; }

    public PasswordHash(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("A password hash cannot be empty.");
        }

        Value = value;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
