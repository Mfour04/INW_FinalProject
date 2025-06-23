using Domain.Enums;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities
{
    public class TransactionEntity : BaseEntity
    {
        public string user_id { get; set; }
        public string novel_id { get; set; }
        public string chapter_id { get; set; }
        public PaymentType type { get; set; }
        public int amount { get; set; }
        public string payment_method { get; set; }
        public PaymentStatus status { get; set; }
        public long completed_at { get; set; }
    }
}