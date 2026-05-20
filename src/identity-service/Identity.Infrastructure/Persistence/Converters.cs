using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Identity.Domain.ValueObjects;

namespace Identity.Infrastructure.Persistence;

internal static class Converters
{
    public static readonly ValueConverter<UserId, Guid> UserId =
        new(id => id.Value, value => new UserId(value));

    public static readonly ValueConverter<Username, string> Username =
        new(username => username.Value, value => new Username(value));

    public static readonly ValueConverter<PasswordHash, string> PasswordHash =
        new(hash => hash.Value, value => new PasswordHash(value));

    public static readonly ValueConverter<Email, string> Email =
        new(email => email.Value, value => new Email(value));

    public static readonly ValueConverter<PersonName, string> PersonName =
        new(name => name.Value, value => new PersonName(value));

    public static readonly ValueConverter<PhoneNumber, string> PhoneNumber =
        new(phone => phone.Value, value => new PhoneNumber(value));
}
