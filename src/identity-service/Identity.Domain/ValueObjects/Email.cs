using System.Text.RegularExpressions;
using Identity.Domain.Common;

namespace Identity.Domain.ValueObjects;

public sealed partial class Email : ValueObject
{
    public string Value { get; }

    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("An email cannot be empty.");
        }

        var normalized = value.Trim().ToLowerInvariant();
        if (!EmailPattern().IsMatch(normalized))
        {
            throw new DomainException("The email format is invalid.");
        }

        Value = normalized;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailPattern();
}
