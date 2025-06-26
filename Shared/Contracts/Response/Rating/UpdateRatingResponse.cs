using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Contracts.Response.Rating
{
    public class UpdateRatingResponse
    {
        public string RatingId { get; set; }
        public int Score { get; set; }
    }
}
