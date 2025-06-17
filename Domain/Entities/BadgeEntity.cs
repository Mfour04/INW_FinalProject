using Domain.Enums;

namespace Domain.Entities
{
    public class BadgeEntity : BaseEntity
    {
        public string name { get; set; }
        public string description { get; set; }
        public string icon_url { get; set; }
        public BadgeTriggerType trigger_type { get; set; }
        public BadgeAction target_action  { get; set; }
        public int required_count { get; set; }
    }
}