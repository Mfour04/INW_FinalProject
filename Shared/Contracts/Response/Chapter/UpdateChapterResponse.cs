using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Contracts.Response.Chapter
{
    public class UpdateChapterResponse
    {
        public string ChapterId { get; set; }
        public string NovelId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public int? ChapterNumber { get; set; }
        public bool? IsPaid { get; set; }
        public int? Price { get; set; }
        public long ScheduledAt { get; set; }
        public bool? IsLock { get; set; }
        public bool AllowComment { get; set; }
        public bool? IsDraft { get; set; }
        public bool? IsPublic { get; set; }
        public int? CommentCount { get; set; }
        public int TotalChapterViews { get; set; }
        public long CreateAt { get; set; }
        public long UpdateAt { get; set; }
    }
}
