using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities
{
    public class TagEntity
    {
        [BsonId]
        [BsonElement("_id")]
        public string id { get; set; }
        public string name { get; set; }
    }
}