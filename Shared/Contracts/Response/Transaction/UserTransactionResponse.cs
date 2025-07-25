using Domain.Enums;

namespace Shared.Contracts.Response.Transaction
{
    public abstract class UserTransactionResponse
    {
        public string Id { get; set; }
        public PaymentType Type { get; set; }
        public int Amount { get; set; }
        public PaymentStatus Status { get; set; }
        public long CreatedAt { get; set; }
    }

    public class TopUpTransactionResponse : UserTransactionResponse
    {
        public string PaymentMethod { get; set; }
        public long CompletedAt { get; set; }
    }

    public class WithdrawTransactionResponse : UserTransactionResponse
    {
        public string BankAccountName { get; set; }
        public string BankAccountNumber { get; set; }
        public string Message { get; set; }
        public long CompletedAt { get; set; }
    }

    public class BuyNovelTransactionResponse : UserTransactionResponse
    {
        public string NovelId { get; set; }
    }

    public class BuyChapterTransactionResponse : UserTransactionResponse
    {
        public string NovelId { get; set; }
        public string ChapterId { get; set; }
    }
}