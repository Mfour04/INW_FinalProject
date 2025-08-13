using Domain.Enums;

namespace Domain.Entities
{
    public class UserEntity : BaseEntity
    {
        public string username { get; set; }
        public string displayname { get; set; }
        public string displayname_unsigned { get; set; }
        public string displayname_normalized { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public string avata_url { get; set; }
        public string cover_url { get; set; }
        public string bio { get; set; }
        public Role role { get; set; } 
        public bool is_verified { get; set; }
        public bool is_banned { get; set; }
        public long? banned_until { get; set; }
        public int coin { get; set; }
        public int block_coin { get; set; }
        public List<string> favourite_type { get; set; } = new();
        public int novel_follow_count { get; set; }
        public int follower_count { get; set; }
        public int following_count { get; set; }
        public List<string> badge_id { get; set; } = new();
        public long last_login { get; set; }
        //public class TagName
        //{
        //    public string id_tag { get; set; }
        //    public string name_tag { get; set; }
        //}
    }
}
