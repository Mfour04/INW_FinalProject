using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Contracts.Response.User
{
    public class UserResponse
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public string AvatarUrl { get; set; }
        public string Bio { get; set; }
        public Role Role { get; set; } 
        public bool isVerified { get; set; }
        public bool isBanned { get; set; }
        public int Coin { get; set; }
        public int BlockCoin { get; set; }
        public int NovelFollowCount { get; set; }
        public List<string> BadgeId { get; set; } = new();
        public long LastLogin { get; set; }
    }
}
