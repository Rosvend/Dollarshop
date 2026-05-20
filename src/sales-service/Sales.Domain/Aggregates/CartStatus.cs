namespace Sales.Domain.Aggregates;

/// <summary>
/// Lifecycle state of a <see cref="ShoppingCart"/>. The aggregate uses it to
/// reject operations that do not make sense for the current state.
/// </summary>
public enum CartStatus
{
    /// <summary>The customer is still composing the cart; items may change.</summary>
    Active,

    /// <summary>Checkout was started; awaiting the payment outcome.</summary>
    CheckedOut,

    /// <summary>Payment was confirmed and the sale was closed.</summary>
    Closed,

    /// <summary>Checkout was reverted (e.g. payment rejected).</summary>
    Reverted,

    /// <summary>The cart was abandoned without completing checkout.</summary>
    Abandoned
}
