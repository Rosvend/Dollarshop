using System.Text.RegularExpressions;
using Identity.Domain.Common;

namespace Identity.Domain.ValueObjects;

public sealed partial class PhoneNumber : ValueObject
{
    public string Value { get; }

    public PhoneNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("A phone number cannot be empty.");
        }

        var normalized = value.Trim();
        if (!PhonePattern().IsMatch(normalized))
        {
            throw new DomainException("The phone number format is invalid.");
        }

        Value = normalized;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    [GeneratedRegex(@"^\+?[0-9][0-9\s\-]{6,20}$")]
    private static partial Regex PhonePattern();
}
