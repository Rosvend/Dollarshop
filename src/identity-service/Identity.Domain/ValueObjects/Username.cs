using System.Text.RegularExpressions;
using Identity.Domain.Common;

namespace Identity.Domain.ValueObjects;

public sealed partial class Username : ValueObject
{
    public string Value { get; }

    public Username(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("A username cannot be empty.");
        }

        var normalized = value.Trim().ToLowerInvariant();
        if (normalized.Length < 3 || normalized.Length > 32)
        {
            throw new DomainException("A username must be between 3 and 32 characters.");
        }

        if (!UsernamePattern().IsMatch(normalized))
        {
            throw new DomainException("A username may only contain letters, digits, dots, underscores and hyphens.");
        }

        Value = normalized;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    [GeneratedRegex("^[a-z0-9._-]+$")]
    private static partial Regex UsernamePattern();
}
