namespace Domain.Entities
{
    public class BadgeEntity : BaseEntity
    {
        public string name { get; set; }
        public string description { get; set; }
        public string icon_url { get; set; }
        public string criteria_type { get; set; }
        public int criteria_value { get; set; }
    }
}