using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Contracts.Response.Comment
{
    public class CommentResponse
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string NovelId { get; set; }
        public string ChapterId { get; set; }
        public string Content { get; set; }
        public string ParentCommentId { get; set; }
        public int LikeCount { get; set; }
        public long CreatedAt { get; set; }
        public long UpdatedAt { get; set; }

        public string UserName { get; set; } 
        public List<CommentResponse> Replies { get; set; } = new List<CommentResponse>(); 
    }
}
