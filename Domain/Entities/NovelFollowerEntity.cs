using Domain.Enums;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities
{
    public class NovelFollowerEntity
    {
        [BsonId]
        [BsonElement("_id")]
        public string id { get; set; }
        public string novel_id { get; set; }
        public string user_id { get; set; }
        public string username { get; set; }
        public bool is_notification { get; set; }
        public NovelFollowReadingStatus reading_status { get; set; } // e.g., "reading", "completed", "dropped"   
        public long followed_at { get; set; }
    }
}
