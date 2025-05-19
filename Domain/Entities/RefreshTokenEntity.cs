namespace Domain.Entities
{
    public class KeyTokenEntity : BaseEntity
    {
        public string token { get; set; }
        public string user_id { get; set; }
    }
}
