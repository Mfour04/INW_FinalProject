
namespace Shared.Contracts.Response.Ownership
{
    public class OwnershipResponse
    {
        public string OwnershipId { get; set; }
        public string UserId { get; set; }
        public string NovelId { get; set; }
        public List<string> ChapterId { get; set; } = new();
        public bool IsFull { get; set; }
    }
}
