using System.Text.RegularExpressions;

namespace Shared.Helpers
{
    public static class CommentContentFilter
    {
        private static readonly List<string> BlacklistWords = new()
        {
            "clm", "địt", "nổ hũ", "cược", "tiền ảo", "link kiếm tiền"
        };

        private static readonly Regex UrlPattern = new(@"(http|www\\.|\\.com|\\.vn|\\.xyz)", RegexOptions.IgnoreCase);

        public static bool ContainsBannedContent(string content)
        {
            var lower = content.ToLower();
            return BlacklistWords.Any(word => lower.Contains(word)) || UrlPattern.IsMatch(content);
        }
    }
}