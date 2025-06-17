using Domain.Enums;

namespace Shared.Contracts.Response.Badge
{
    public class BadgeResponse
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string IconUrl { get; set; }
        public BadgeTriggerType TriggerType { get; set; }
        public BadgeAction TargetAction { get; set; }
        public int RequiredCount { get; set; }
        public long CreatedAt { get; set; }
    }
}