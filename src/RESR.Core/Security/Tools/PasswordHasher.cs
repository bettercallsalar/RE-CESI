using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace RESR.Core.Security.Tools;

public sealed class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int IterationCount = 10000;

    public string HashPassword(string password)
    {
        var salt = new byte[SaltSize];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(salt);

        var hash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: IterationCount,
            numBytesRequested: HashSize
        ));

        return $"{Convert.ToBase64String(salt)}.{hash}";
    }

    public bool VerifyPassword(string hashedPassword, string providedPassword)
    {
        if (string.IsNullOrWhiteSpace(hashedPassword))
            return false;

        // Backward compatibility for legacy SHA256 hashes already stored in the DB.
        if (!hashedPassword.Contains('.'))
            return VerifyLegacySha256(hashedPassword, providedPassword);

        var parts = hashedPassword.Split('.', 2);
        if (parts.Length != 2)
            return false;

        byte[] salt;
        try
        {
            salt = Convert.FromBase64String(parts[0]);
        }
        catch (FormatException)
        {
            return false;
        }

        var expectedHash = parts[1];
        var providedHash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: providedPassword,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: IterationCount,
            numBytesRequested: HashSize
        ));

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(expectedHash),
            Encoding.UTF8.GetBytes(providedHash)
        );
    }

    private static bool VerifyLegacySha256(string storedHexHash, string providedPassword)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(providedPassword));
        var providedHex = Convert.ToHexString(bytes);

        return string.Equals(storedHexHash, providedHex, StringComparison.OrdinalIgnoreCase);
    }
}
