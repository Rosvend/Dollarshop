namespace Sales.Domain.Common;

/// <summary>
/// Raised when a domain invariant is violated. Value Objects and the aggregate
/// throw this from their constructors and behavior methods so an invalid model
/// can never come into existence.
/// </summary>
public sealed class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }
}
