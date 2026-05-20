using Identity.Domain.Common;
using Identity.Domain.Interfaces;
using Identity.Domain.ValueObjects;

namespace Identity.Domain.Aggregates;

/// <summary>
/// Aggregate root of the Identity bounded context. Guards registration data
/// invariants and credential changes.
/// </summary>
public sealed class User : AggregateRoot<UserId>
{
    public Username Username { get; }

    public PasswordHash PasswordHash { get; private set; }

    public Email Email { get; }

    public PersonName Name { get; }

    public PhoneNumber Phone { get; }

    public RegistrationData Profile => new(Email, Name, Phone);

    public bool Active { get; private set; }

    public User(
        UserId id,
        Username username,
        PasswordHash passwordHash,
        RegistrationData profile)
        : this(id, username, passwordHash, profile.Email, profile.Name, profile.Phone)
    {
    }

    internal User(
        UserId id,
        Username username,
        PasswordHash passwordHash,
        Email email,
        PersonName name,
        PhoneNumber phone) : base(id)
    {
        Username = username;
        PasswordHash = passwordHash;
        Email = email;
        Name = name;
        Phone = phone;
        Active = true;
    }

    public bool Authenticate(string plainText, IPasswordVerifier verifier)
    {
        if (!Active)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(plainText))
        {
            return false;
        }

        return verifier.Verify(plainText, PasswordHash);
    }

    public void ChangePassword(string newPlainText, IPasswordVerifier verifier)
    {
        if (!Active)
        {
            throw new DomainException("Cannot change the password of an inactive user.");
        }

        if (string.IsNullOrWhiteSpace(newPlainText) || newPlainText.Length < 6)
        {
            throw new DomainException("A password must be at least 6 characters.");
        }

        PasswordHash = verifier.Hash(newPlainText);
    }

    public void Deactivate()
    {
        Active = false;
    }
}
