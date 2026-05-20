namespace Sales.Application.Dtos;

/// <summary>Transport shape of a monetary amount. Maps to the domain <c>Money</c> VO.</summary>
public sealed record MoneyDto(decimal Amount, string Currency);
