using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Contracts.Response.Forum
{
    public class CheckFollowStatusResponse
    {
        public bool IsFollowing { get; set; }
        public bool IsFollowedBy { get; set; }
    }
}
