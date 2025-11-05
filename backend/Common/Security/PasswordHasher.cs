using System.Security.Cryptography;
using System.Text;

namespace Common.Security;

public static class PasswordHasher
{
    public static string Hash(string plain)
    {
        using var sha = SHA256.Create();
        return Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(plain)));
    }

    public static bool Verify(string plain, string hash) => Hash(plain) == hash;
}