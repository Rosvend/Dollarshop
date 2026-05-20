namespace Sales.Domain.ValueObjects;

/// <summary>
/// The currencies Dollarshop operates with. Modeling currency as a closed enum
/// (instead of a raw string) removes primitive obsession: only declared
/// currencies are representable, and a typo cannot reach the domain.
/// </summary>
public enum Currency
{
    /// <summary>United States dollar.</summary>
    USD,

    /// <summary>Colombian peso.</summary>
    COP,

    /// <summary>Euro.</summary>
    EUR
}
