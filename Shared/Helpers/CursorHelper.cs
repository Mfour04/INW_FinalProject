using System.Security.Cryptography;
using System.Text;

namespace Shared.Helpers
{
    public static class CursorHelper
    {
        private const int DefaultTtlMinutes = 30;

        public static string MakeShortCursor20(string novelId, long createdAtTicks, int? ttlMinutesOverride = null)
        {
            var expUnix = TimeHelper.AddMinutes(ttlMinutesOverride ?? DefaultTtlMinutes).ToUnixTimeSeconds();
            var ticks36 = ToBase36(createdAtTicks).PadLeft(12, '0');
            var exp36 = ToBase36(expUnix).PadLeft(7, '0');
            var chk = CalcNovelCheckChar(novelId);
            return ticks36 + exp36 + chk;
        }

        public static bool TryParseShortCursor20(string? afterId, string novelId, out long createdAtTicks, out bool isExpired)
        {
            createdAtTicks = 0;
            isExpired = false;

            if (string.IsNullOrWhiteSpace(afterId) || afterId.Length != 20) return false;

            var ticks36 = afterId.Substring(0, 12);
            var exp36 = afterId.Substring(12, 7);
            var chk = afterId[19];

            var expected = CalcNovelCheckChar(novelId);
            if (chk != expected) return false;

            if (!TryFromBase36(ticks36, out var ticks)) return false;
            if (!TryFromBase36(exp36, out var expUnix)) return false;

            createdAtTicks = ticks;
            isExpired = TimeHelper.NowUnixTimeSeconds > expUnix;

            return !isExpired;
        }

        private static char CalcNovelCheckChar(string novelId)
        {
            var up = (novelId ?? string.Empty).Trim().ToUpperInvariant();
            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(up));
            var v = hash[0] % 36;
            return Base36Digit(v);
        }

        private static string ToBase36(long value)
        {
            if (value == 0) return "0";
            var sb = new StringBuilder();
            var v = value;
            var neg = v < 0;
            if (neg) v = -v;

            while (v > 0)
            {
                var rem = (int)(v % 36);
                sb.Insert(0, Base36Digit(rem));
                v /= 36;
            }
            if (neg) sb.Insert(0, '-');
            return sb.ToString();
        }

        private static bool TryFromBase36(string s, out long value)
        {
            value = 0;
            if (string.IsNullOrWhiteSpace(s)) return false;

            var i = 0;
            var neg = false;
            if (s[0] == '-')
            {
                neg = true; i = 1;
                if (s.Length == 1) return false;
            }
            for (; i < s.Length; i++)
            {
                var c = s[i];
                int digit;
                if (c >= '0' && c <= '9') digit = c - '0';
                else if (c >= 'a' && c <= 'z') digit = c - 'a' + 10;
                else if (c >= 'A' && c <= 'Z') digit = c - 'A' + 10;
                else return false;

                if (digit >= 36) return false;

                checked { value = value * 36 + digit; }
            }
            if (neg) value = -value;
            return true;
        }

        private static char Base36Digit(int d)
        {
            return d < 10 ? (char)('0' + d) : (char)('a' + (d - 10));
        }
    }
}
