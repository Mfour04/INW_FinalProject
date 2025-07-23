using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities.OpenAIEntity
{
    public class NovelEmbeddingEntity
    {
        [BsonId]
        [BsonElement("_id")]
        public string novel_id { get; set; }
        public List<float> vector_novel { get; set; }
        public long updated_at { get; set; }
    }
}
