using System.Security.Cryptography;
using System.Text;

namespace Application.Auth;

public static class TelegramLoginVerifier
{
    public static bool Verify(IDictionary<string, string> authData, string botToken)
    {
        if (!authData.TryGetValue("hash", out var hash))
            return false;

        var dataCheck = string.Join("\n",
            authData
                .Where(kv => kv.Key != "hash")
                .OrderBy(kv => kv.Key)
                .Select(kv => $"{kv.Key}={kv.Value}"));

        using var hmac = new HMACSHA256(SHA256.HashData(Encoding.UTF8.GetBytes(botToken)));
        var computed = hmac.ComputeHash(Encoding.UTF8.GetBytes(dataCheck));
        var computedHex = BitConverter.ToString(computed).Replace("-", "").ToLowerInvariant();
        return computedHex == hash.ToLowerInvariant();
    }
}