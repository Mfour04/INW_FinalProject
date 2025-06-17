namespace Domain.Entities
{
    public class BadgeProgressEntity : BaseEntity
    {
        public string user_id { get; set; }
        public string badge_id { get; set; }
        public int current_value { get; set; }
        public bool is_completed { get; set; } 
        public long completed_at { get; set; }
    }
}