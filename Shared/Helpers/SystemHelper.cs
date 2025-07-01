using Domain.Entities.System;
using System.Globalization;
using System.Net.NetworkInformation;
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

        public static List<string> ParseSearchQuery(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new List<string>();
            return query.Split(' ', '+').Select(RemoveDiacritics).ToList();
        }

        public static (string Exact, List<string> FuzzyTerms) ParseSearchQuerySmart(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return ("", new List<string>());

            string normalized = RemoveDiacritics(query).ToLower().Trim();
            var fuzzy = normalized.Split(' ').Where(s => !string.IsNullOrWhiteSpace(s)).ToList();

            return (normalized, fuzzy);
        }

        public static List<SortCreterias> ParseSortCriteria(string sortBy)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
                return new List<SortCreterias>();

            return sortBy
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(field => field.Split(':', StringSplitOptions.RemoveEmptyEntries))
                .Where(parts => parts.Length == 2)
                .Select(parts => new SortCreterias
                {
                    Field = parts[0].Trim(),
                    IsDescending = parts[1].Equals("desc", StringComparison.OrdinalIgnoreCase),
                })
                .ToList();  
        }

        public static List<string> ParseTagNames(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return new List<string>();

            return raw
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(tag => tag.Trim())
                .Where(tag => !string.IsNullOrWhiteSpace(tag))
                .ToList();
        }
    }
}