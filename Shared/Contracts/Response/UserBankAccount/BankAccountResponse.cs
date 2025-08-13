namespace Shared.Contracts.Response.UserBankAccount
{
    public abstract class BaseBankAccountResponse
    {
        public string Id { get; set; }
        public string BankShortName { get; set; }
        public long BankAccountNumber { get; set; }
        public string BankAccountName { get; set; }
        public bool IsDefault { get; set; }
        public long CreatedAt { get; set; }
    }

    public class BankAccountResponse : BaseBankAccountResponse { }
}