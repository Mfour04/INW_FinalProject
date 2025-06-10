namespace Domain.Entities
{
    public class NotificationEntity : BaseEntity
    {
        public string user_id { get; set; }
        public string type { get; set; }
        public string message { get; set; }
        public bool is_read { get; set; }
    }
}