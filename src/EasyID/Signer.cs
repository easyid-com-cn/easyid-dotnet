using System.Security.Cryptography;
using System.Text;

namespace EasyID;

internal static class Signer
{
    public static string Sign(string secret, string timestamp, IReadOnlyDictionary<string, string>? query, byte[] body)
    {
        var parts = new List<string>();
        if (query is not null)
        {
            foreach (var pair in query.OrderBy(item => item.Key, StringComparer.Ordinal))
            {
                parts.Add($"{pair.Key}={pair.Value}");
            }
        }

        var payload = string.Join("&", parts);
        if (body.Length > 0)
        {
            var bodyText = Encoding.UTF8.GetString(body);
            payload = string.IsNullOrEmpty(payload) ? bodyText : $"{payload}&{bodyText}";
        }

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        return Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes($"{timestamp}\n{payload}"))).ToLowerInvariant();
    }
}
