using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities.OpenAIEntity
{
    public class UserEmbeddingEntity
    {
        [BsonId]
        [BsonElement("_id")]
        public string user_id { get; set; }
        public List<float> vector_user { get; set; }
        public long updated_at { get; set; }
    }
}
