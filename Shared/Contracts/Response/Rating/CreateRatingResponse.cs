using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Contracts.Response.Rating
{
    public class CreateRatingResponse
    {
        public string UserId { get; set; }
        public string NovelId { get; set; }
        public int Score { get; set; }
    }
}
