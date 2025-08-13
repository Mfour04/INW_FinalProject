using Domain.Enums;

namespace Domain.Entities
{
    public class NotificationEntity : BaseEntity
    {
        public string user_id { get; set; }
        public NotificationType type { get; set; }
        public string message { get; set; }
        public bool is_read { get; set; }
    }
}