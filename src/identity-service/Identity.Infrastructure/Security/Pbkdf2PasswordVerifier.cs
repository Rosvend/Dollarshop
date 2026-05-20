using System.Security.Cryptography;
using System.Text;
using Identity.Domain.Interfaces;
using Identity.Domain.ValueObjects;

namespace Identity.Infrastructure.Security;

/// <summary>PBKDF2 password hashing — infrastructure implementation of the domain port.</summary>
public sealed class Pbkdf2PasswordVerifier : IPasswordVerifier
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 100_000;

    public PasswordHash Hash(string plainText)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var key = Rfc2898DeriveBytes.Pbkdf2(
            plainText,
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            KeySize);

        return new PasswordHash(Encode(salt, key));
    }

    public bool Verify(string plainText, PasswordHash storedHash)
    {
        var parts = Decode(storedHash.Value);
        var key = Rfc2898DeriveBytes.Pbkdf2(
            plainText,
            parts.Salt,
            Iterations,
            HashAlgorithmName.SHA256,
            KeySize);

        return CryptographicOperations.FixedTimeEquals(key, parts.Key);
    }

    private static string Encode(byte[] salt, byte[] key) =>
        $"pbkdf2|{Iterations}|{Convert.ToBase64String(salt)}|{Convert.ToBase64String(key)}";

    private static (byte[] Salt, byte[] Key) Decode(string stored)
    {
        var parts = stored.Split('|');
        if (parts.Length != 4 || parts[0] != "pbkdf2")
        {
            throw new InvalidOperationException("Unrecognized password hash format.");
        }

        return (Convert.FromBase64String(parts[2]), Convert.FromBase64String(parts[3]));
    }
}
