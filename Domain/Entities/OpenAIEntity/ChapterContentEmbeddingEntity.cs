using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.OpenAIEntity
{
    public class ChapterContentEmbeddingEntity
    {
        [BsonId]
        [BsonElement("_id")]
        public string chapter_id { get; set; }
        public List<float> vector_chapter_content { get; set; }
        public long updated_at { get; set; }
    }
}
