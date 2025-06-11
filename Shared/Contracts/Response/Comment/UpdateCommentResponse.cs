using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Contracts.Response.Comment
{
    public class UpdateCommentResponse
    {
        public string CommentId { get; set; }
        public string Content { get; set; }
    }
}
