using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Contracts.Response.Rating
{
    public class RatingResponse
    {
        public string RatingId { get; set; }
        public string UserId { get; set; }
        public string DisplayName { get; set; }
        public string NovelId { get; set; }
        public int Score { get; set; }
        public string RatingContent { get; set; } 
        public long CreatedAt { get; set; }
        public long UpdatedAt { get; set; }
    }
}
