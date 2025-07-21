using Domain.Enums;

namespace Shared.Contracts.Response.Transaction
{
    public abstract class AdminTransactionResponse
    {
        public string Id { get; set; }
        public string RequesterId { get; set; }
        public PaymentType Type { get; set; }
        public int Amount { get; set; }
        public PaymentStatus Status { get; set; }
        public long CreatedAt { get; set; }
        public long UpdatedAt { get; set; }
        public long CompletedAt { get; set; }
    }

    public class AdminTopUpTransactionResponse : AdminTransactionResponse
    {
        public string PaymentMethod { get; set; }
    }

    public class AdminWithdrawTransactionResponse : AdminTransactionResponse
    {
        public string BankAccountName { get; set; }
        public string BankAccountNumber { get; set; }
        public string ActionById { get; set; }
        public string ActionType { get; set; }
        public string Message { get; set; }
        public string PaymentMethod { get; set; }
    }

    public class AdminBuyNovelTransactionResponse : AdminTransactionResponse
    {
        public string NovelId { get; set; }
    }

    public class AdminBuyChapterTransactionResponse : AdminTransactionResponse
    {
        public string NovelId { get; set; }
        public string ChapterId { get; set; }
    }
}