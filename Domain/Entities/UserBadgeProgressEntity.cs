namespace Domain.Entities
{
    public class UserBadgeProgressEntity : BaseEntity
    {
        public string user_id { get; set; }
        public string badge_id { get; set; }
        public int progress { get; set; }
        public bool is_completed { get; set; }
        public long completed_at { get; set; }
    }
}