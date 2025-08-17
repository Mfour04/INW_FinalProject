using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Shared.Helpers
{
    public static class CursorHmacHelper
    {
        private static readonly byte[] _secret = Encoding.UTF8.GetBytes("strong-secret-change-me");

        private sealed class CursorPayload
        {
            public long t { get; set; }     
            public string i { get; set; } = "";
        }

        public static string Make(string novelId, long ticks, string id, int ttlMinutes = 30)
        {
            var payload = new CursorPayload { t = ticks, i = id ?? "" };
            var json = JsonSerializer.Serialize(payload);

            var data = Base64UrlEncode(Encoding.UTF8.GetBytes(json));

            var sigFull = ComputeHmacSha256(Encoding.UTF8.GetBytes(data + novelId), _secret);
            var sigShort = new byte[10];
            Array.Copy(sigFull, sigShort, 10);
            var sig = Base64UrlEncode(sigShort);

            return $"{data}.{sig}";
        }

        public static bool TryParse(string? cursor, string novelId, out long ticks, out string? id)
        {
            ticks = 0; id = null;
            if (string.IsNullOrWhiteSpace(cursor)) return false;

            var parts = cursor.Split('.');
            if (parts.Length != 2) return false;

            var data = parts[0];
            var sig = parts[1];

            var expectedFull = ComputeHmacSha256(Encoding.UTF8.GetBytes(data + novelId), _secret);
            var expectedShort = new byte[10];
            Array.Copy(expectedFull, expectedShort, 10);
            var expectedSig = Base64UrlEncode(expectedShort);

            if (!CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(sig), Encoding.UTF8.GetBytes(expectedSig)))
                return false;

            var json = Encoding.UTF8.GetString(Base64UrlDecode(data));
            var payload = JsonSerializer.Deserialize<CursorPayload>(json);
            if (payload == null) return false;

            ticks = payload.t;
            id = payload.i;
            return true;
        }

        private static byte[] ComputeHmacSha256(byte[] msg, byte[] key)
        {
            using var h = new HMACSHA256(key);
            return h.ComputeHash(msg);
        }

        private static string Base64UrlEncode(byte[] bytes)
            => Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

        private static byte[] Base64UrlDecode(string s)
        {
            s = s.Replace('-', '+').Replace('_', '/');
            switch (s.Length % 4)
            {
                case 2: s += "=="; break;
                case 3: s += "="; break;
            }
            return Convert.FromBase64String(s);
        }
    }
}
