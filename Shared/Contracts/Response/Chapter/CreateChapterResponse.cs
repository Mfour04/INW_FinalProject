using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Shared.Contracts.Response.Chapter
{
    public class CreateChapterResponse
    {
        public string NovelId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public int ChapterNumber { get; set; }
        public long ScheduleAt { get; set; }
        public bool? IsPaid { get; set; }
        public int? Price { get; set; }
        public bool? IsDraft { get; set; }
        public bool? IsPublic { get; set; }
    }
}
