using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Contracts.Response.Comment
{
    public class CreateCommentResponse
    {
        public string UserId { get; set; }
        public string NovelId { get; set; }
        public string ChapterId { get; set; }
        public string Content { get; set; }
        public string ParentCommentId { get; set; }
    }
}
