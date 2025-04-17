using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Users
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; }

        [BsonElement("username")]
        public string? Username { get; set; }

        [BsonElement("email")]
        public string? Email { get; set; }

        [BsonElement("passwordHash")]
        public string? PasswordHash { get; set; }

        [BsonElement("role")]
        public string? Role { get; set; } // reader, author, admin

        [BsonElement("avatarUrl")]
        public string? AvatarUrl { get; set; }

        [BsonElement("bio")]
        public string? Bio { get; set; }

        [BsonElement("coin")]
        public int Coin { get; set; }

        //[BsonElement("followedNovels")]
        //public List<FollowedNovel> FollowedNovels { get; set; } = new List<FollowedNovel>();

        [BsonElement("badgeId")]
        public List<string> BadgeId { get; set; } = new List<string>(); // ref tới Badges._id

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; }

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; }
    }
}
