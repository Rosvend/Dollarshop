using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Sales.Application.Dtos;
using Sales.Application.Mapping;
using Sales.Domain.ValueObjects;
using Sales.Domain.ValueObjects.Discounts;
using Sales.Infrastructure.Mapping;
using Sales.Infrastructure.Serialization;

namespace Sales.Infrastructure.Persistence;

/// <summary>
/// EF Core <see cref="ValueConverter"/>s — the heart of the ORM mapping that lets
/// the persistence layer store the rich domain model without the Domain ever
/// knowing about a database:
/// <list type="bullet">
///   <item>the strongly-typed id Value Objects collapse to a <see cref="Guid"/> column;</item>
///   <item><see cref="ProductReference"/> (and its nested <c>Money</c>) is stored as
///   a single <c>jsonb</c> column — it is a value-converted scalar, so EF can bind it
///   through <c>CartItem</c>'s constructor (owned types cannot be constructor-bound);</item>
///   <item>the polymorphic <see cref="DiscountPolicy"/> is stored as <c>jsonb</c> via the
///   existing discriminated <see cref="DiscountSpecDto"/>.</item>
/// </list>
/// </summary>
internal static class Converters
{
    public static readonly ValueConverter<CartId, Guid> CartId =
        new(id => id.Value, value => new CartId(value));

    public static readonly ValueConverter<CartItemId, Guid> CartItemId =
        new(id => id.Value, value => new CartItemId(value));

    public static readonly ValueConverter<CustomerId, Guid> CustomerId =
        new(id => id.Value, value => new CustomerId(value));

    public static readonly ValueConverter<Quantity, int> Quantity =
        new(quantity => quantity.Value, value => new Quantity(value));

    public static readonly ValueConverter<ProductReference, string> ProductReference =
        new(
            reference => JsonSerializer.Serialize(reference, JsonDefaults.Options),
            json => JsonSerializer.Deserialize<ProductReference>(json, JsonDefaults.Options)!);

    public static readonly ValueConverter<DiscountPolicy, string> DiscountPolicy =
        new(
            policy => JsonSerializer.Serialize(DiscountPolicyDtoMapper.ToDto(policy), JsonDefaults.Options),
            json => DiscountPolicyMapper.ToDomain(
                JsonSerializer.Deserialize<DiscountSpecDto>(json, JsonDefaults.Options)!));
}
