using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Contracts.Response
{
    public class NovelResponse
    {
        public string NovelId { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public string AuthorId { get; set; }
        public List<string>? Genres { get; set; }
        public List<string>? Tags { get; set; }
        public string Status { get; set; }
        public bool IsPremium { get; set; }
        public double Price { get; set; }
        public int TotalViews { get; set; }
        public int TotalChapters { get; set; }
        public float RatingAvg { get; set; }
        public int RatingCount { get; set; }
        public int Followers { get; set; }
    }
}
