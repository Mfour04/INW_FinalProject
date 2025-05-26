using Domain.Enums;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities
{
    public class TransactionEntity
    {
        [BsonId]
        [BsonElement("_id")]
        public string id { get; set; }
        public string user_id { get; set; }
        public string novel_id { get; set; }
        public string chapter_id { get; set; }
        public string type { get; set; }
        public int amount { get; set; }
        public string payment_method { get; set; }
        public PaymentStatus status { get; set; }
        public long created_at { get; set; }
    }
}