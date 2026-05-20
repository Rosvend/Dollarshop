using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Catalog.Domain.ValueObjects;

namespace Catalog.Infrastructure.Persistence;

internal static class Converters
{
    public static readonly ValueConverter<ProductId, Guid> ProductId =
        new(id => id.Value, value => new ProductId(value));

    public static readonly ValueConverter<Sku, string> Sku =
        new(sku => sku.Value, value => new Sku(value));

    public static readonly ValueConverter<ProductName, string> ProductName =
        new(name => name.Value, value => new ProductName(value));

    public static readonly ValueConverter<Money, string> Money =
        new(
            money => $"{money.Amount}|{money.Currency}",
            stored => ParseMoney(stored));

    public static readonly ValueConverter<StockLevel, int> StockLevel =
        new(stock => stock.OnHand, value => new StockLevel(value));

    private static Money ParseMoney(string stored)
    {
        var parts = stored.Split('|', 2);
        return new Money(
            decimal.Parse(parts[0]),
            Enum.Parse<Currency>(parts[1], ignoreCase: true));
    }
}
