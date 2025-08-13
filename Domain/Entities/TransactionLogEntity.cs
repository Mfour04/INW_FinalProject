namespace Domain.Entities
{
    public class TransactionLogEntity : BaseEntity
    {
        public string transaction_id { get; set; }
        public string action_by_id { get; set; }
        public string action_type { get; set; }
        public string message { get; set; }
    }
}