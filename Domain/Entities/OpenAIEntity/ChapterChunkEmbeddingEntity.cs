using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.OpenAIEntity
{
    public class ChapterChunkEmbeddingEntity
    {
        [BsonId]
        [BsonElement("_id")]
        public string chunk_id { get; set; }

        public string chapter_id { get; set; }
        public string novel_id { get; set; }

        public int chunk_index { get; set; }
        public string chunk_text { get; set; }

        public List<float> vector_chunk_content { get; set; }

        public long created_at { get; set; }
    }
}
