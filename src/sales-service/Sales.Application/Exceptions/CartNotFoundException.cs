namespace Sales.Application.Exceptions;

/// <summary>
/// Raised when a use case references a cart that does not exist. This is an
/// orchestration concern (a lookup miss), not a domain invariant violation.
/// </summary>
public sealed class CartNotFoundException : Exception
{
    public CartNotFoundException(Guid cartId)
        : base($"No shopping cart was found with id '{cartId}'.")
    {
        CartId = cartId;
    }

    public Guid CartId { get; }
}
