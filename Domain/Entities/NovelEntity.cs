using Domain.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities
{
    public class NovelEntity : BaseEntity
    {
        public string title { get; set; }
        public string title_unsigned { get; set; }
        public string slug { get; set; }
        public string description { get; set; }
        public string author_id { get; set; }
        public string novel_image { get; set; }
        public string novel_banner { get; set; }
        public List<string> tags { get; set; } = new();
        [BsonRepresentation(BsonType.String)]
        public NovelStatus status { get; set; }
        public bool is_public { get; set; }
        public bool is_lock { get; set; }
        public bool is_paid { get; set; }
        public bool is_completed { get; set; }
        public int price { get; set; }
        public int total_chapters { get; set; }
        public int total_views { get; set; }
        public int followers { get; set; }
        public double rating_avg { get; set; }
        public int rating_count { get; set; }
        public int comment_count { get; set; }
    }
}