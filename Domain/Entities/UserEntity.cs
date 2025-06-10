using Domain.Enums;

namespace Domain.Entities
{
    public class UserEntity : BaseEntity
    {
        public string username { get; set; }
        public string displayname { get; set; }
        public string displayname_normalized { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public string avata_url { get; set; }
        public string bio { get; set; }
        public Role role { get; set; } // reader, author, admin
        public bool is_verified { get; set; }
        public bool is_banned { get; set; }
        public int coin { get; set; }
        public int novel_follow_count { get; set; }
        public List<string> badge_id { get; set; } = new(); 
        public long last_login { get; set; }
    }
}
