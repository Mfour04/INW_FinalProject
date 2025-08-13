namespace Domain.Entities
{
    public class UserBankAccountEntity : BaseEntity
    {
        public string user_id { get; set; }
        public int bank_bin { get; set; }
        public string bank_code { get; set; }
        public string bank_short_name { get; set; }
        public long bank_account_number { get; set; }
        public string bank_account_name { get; set; }
        public bool is_default { get; set; }
    }
}