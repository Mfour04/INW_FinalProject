
namespace Shared.Contracts.Response.Ownership
{
    public class PurchaserResponse
    {
        public string UserId { get; set; }
        public string NovelId { get; set; }
        public List<string> ChapterId { get; set; } = new();
        public bool IsFull { get; set; }
    }
}
