using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Contracts.Respone
{
    public class UserResponse
    {
        public string UserId { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Bio { get; set; }
        public int Coin { get; set; }
        public List<string> BadgeId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
