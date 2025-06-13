using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Contracts.Response.User
{
    public class UpdateUserReponse
    {
        public string UserId { get; set; }
        public string DisplayName { get; set; }
        public string AvataUrl { get; set; }
        public string Bio { get; set; }
        public List<string> BadgeId { get; set; } = new();
    }
}
