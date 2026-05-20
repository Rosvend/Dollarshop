using Identity.Domain.Common;

namespace Identity.Domain.ValueObjects;

public sealed class RegistrationData : ValueObject
{
    public Email Email { get; }

    public PersonName Name { get; }

    public PhoneNumber Phone { get; }

    public RegistrationData(Email email, PersonName name, PhoneNumber phone)
    {
        Email = email ?? throw new DomainException("Registration data requires an email.");
        Name = name ?? throw new DomainException("Registration data requires a name.");
        Phone = phone ?? throw new DomainException("Registration data requires a phone number.");
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Email;
        yield return Name;
        yield return Phone;
    }
}
