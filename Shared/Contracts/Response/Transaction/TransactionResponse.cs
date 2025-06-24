using Domain.Enums;

namespace Shared.Contracts.Response.Transaction
{
    public class TransactionResponse
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string NovelId { get; set; }
        public string ChapterId { get; set; }
        public PaymentType Type { get; set; }
        public int Amount { get; set; }
        public string PaymentMethod { get; set; }
        public PaymentStatus Status { get; set; }
        public long CreatedAt { get; set; }
        public long UpdatedAt { get; set; }
        public long CompletedAt { get; set; }
    }
}