using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities
{
    public class UserFollowEntity
    {
        [BsonId]
        [BsonElement("_id")]
        public string id { get; set; }
        public string follower_id { get; set; }
        public string following_id { get; set; }
        public long followed_at { get; set; }
    }
}