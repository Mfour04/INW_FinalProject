using System.Globalization;
using System.Text;

namespace Shared.Helpers
{
    public static class SystemHelper
    {
        private static readonly string Characters = "0123456789abcdefghijklmnopqrstuvwxyz";

        public static string RandomId()
        {
            DateTime date = DateTime.Now;

            var result = new StringBuilder();
            result.Append(DateTime.Now.ToString("yy"));
            result.Append(Characters[date.Month]);
            result.Append(Characters[date.Day]);

            var uid = Guid.NewGuid().ToString().Replace("-", "");
            var random = new Random();
            var length = 20;

            result.Append(uid.Substring(random.Next(0, uid.Length - length - 1), length));

            return result.ToString().ToUpper();
        }

        public static string RemoveDiacritics(string input)
        {
            string normalized = input.Normalize(NormalizationForm.FormD);
            char[] chars = normalized
                .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                .ToArray();
            return new string(chars).Normalize(NormalizationForm.FormC).ToLower();
        }
    }
}