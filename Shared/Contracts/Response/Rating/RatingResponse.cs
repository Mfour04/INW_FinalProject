using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Contracts.Response.Rating
{
    public class RatingResponse
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string NovelId { get; set; }
        public int Score { get; set; }
        public string RatingContent { get; set; } 
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
