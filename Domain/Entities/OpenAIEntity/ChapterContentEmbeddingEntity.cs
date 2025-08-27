using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities.OpenAIEntity
{
    public class ChapterContentEmbeddingEntity
    {
        [BsonId]
        [BsonElement("_id")]
        public string chapter_id { get; set; }
        public string novel_id { get; set; }
        public List<float> vector_chapter_content { get; set; }
        public long updated_at { get; set; }
    }
}
    