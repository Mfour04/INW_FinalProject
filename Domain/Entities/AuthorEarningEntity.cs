using Domain.Enums;

namespace Domain.Entities
{
    public class AuthorEarningEntity : BaseEntity
    {
        public string author_id { get; set; }
        public string novel_id { get; set; }
        public string chapter_id { get; set; }
        public int amount { get; set; }
        public PaymentType type { get; set; }
        public string source_transaction_id { get; set; }
    }
}