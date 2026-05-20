using System.Text.Json;
using System.Text.Json.Serialization;
using Sales.Domain.ValueObjects;

namespace Sales.Infrastructure.Serialization;

/// <summary>
/// A single, shared <see cref="JsonSerializerOptions"/> used everywhere the
/// Infrastructure layer turns domain objects into JSON: the EF Core value
/// converters (<c>ProductReference</c>, <c>DiscountPolicy</c>), the Outbox
/// payloads and the ACL messages.
/// <para>
/// The strongly-typed id Value Objects are written as bare GUID strings instead
/// of <c>{"Value":"..."}</c>, so persisted columns and broker payloads stay
/// clean and easy to read.
/// </para>
/// </summary>
internal static class JsonDefaults
{
    public static JsonSerializerOptions Options { get; } = Build();

    private static JsonSerializerOptions Build()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

        options.Converters.Add(new GuidIdJsonConverter<CartId>(g => new CartId(g), id => id.Value));
        options.Converters.Add(new GuidIdJsonConverter<CartItemId>(g => new CartItemId(g), id => id.Value));
        options.Converters.Add(new GuidIdJsonConverter<ProductId>(g => new ProductId(g), id => id.Value));
        options.Converters.Add(new GuidIdJsonConverter<CustomerId>(g => new CustomerId(g), id => id.Value));
        options.Converters.Add(new JsonStringEnumConverter());

        return options;
    }
}

/// <summary>
/// Reads/writes a GUID-backed identifier Value Object as a bare JSON string.
/// </summary>
internal sealed class GuidIdJsonConverter<T> : JsonConverter<T>
    where T : class
{
    private readonly Func<Guid, T> _factory;
    private readonly Func<T, Guid> _accessor;

    public GuidIdJsonConverter(Func<Guid, T> factory, Func<T, Guid> accessor)
    {
        _factory = factory;
        _accessor = accessor;
    }

    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        _factory(reader.GetGuid());

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) =>
        writer.WriteStringValue(_accessor(value));
}
