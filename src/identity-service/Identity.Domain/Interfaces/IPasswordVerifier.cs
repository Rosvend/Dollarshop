using Identity.Domain.ValueObjects;

namespace Identity.Domain.Interfaces;

/// <summary>Domain port for comparing a plaintext password with a stored hash.</summary>
public interface IPasswordVerifier
{
    bool Verify(string plainText, PasswordHash storedHash);

    PasswordHash Hash(string plainText);
}
