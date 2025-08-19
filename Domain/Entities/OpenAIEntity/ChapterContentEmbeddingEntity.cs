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
        public string novel_id { get; set; }
        public string novel_title { get; set; }
        public string chapter_title { get; set; }
        public string slug { get; set; }
        public List<float> vector_chapter_content { get; set; }
        public string chapter_content { get; set; }
        public long updated_at { get; set; }
    }
}
