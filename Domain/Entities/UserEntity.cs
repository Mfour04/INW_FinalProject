using Domain.Enums;

namespace Domain.Entities
{
    public class UserEntity : BaseEntity
    {
        public string user_name { get; set; }
        public string user_name_normalized { get; set; }
        public string email { get; set; }
        public string password_hash { get; set; }
        public Role role { get; set; } // reader, author, admin
        public string avata_url { get; set; }
        public string bio { get; set; }
        public int coin { get; set; }

        //[BsonElement("followedNovels")]
        //public List<FollowedNovel> FollowedNovels { get; set; } = new List<FollowedNovel>();

        public List<string> badge_id { get; set; } = new(); // ref tới Badges._id
    }
}
