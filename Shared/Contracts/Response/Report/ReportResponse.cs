using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Contracts.Response.Report
{
    public class ReportResponse
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string MemberId { get; set; }
        public string NovelId { get; set; }
        public string ChapterId { get; set; }
        public string CommentId { get; set; }
        public string ForumPostId { get; set; }
        public string ForumCommentId { get; set; }
        public ReportTypeStatus Type { get; set; }
        public string Reason { get; set; }
        public ReportStatus Status { get; set; }
        public long CreatedAt { get; set; }
        public long UpdatedAt { get; set; }
    }
}
