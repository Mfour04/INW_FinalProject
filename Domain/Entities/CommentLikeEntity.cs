using Domain.Enums;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities
{
    public class CommentLikeEntity
    {
        [BsonId]
        [BsonElement("_id")]
        public string id { get; set; }
        public string comment_id { get; set; }
        public string user_id { get; set; }
        public CommentType type { get; set; }
        public long like_at { get; set; }
    }
}