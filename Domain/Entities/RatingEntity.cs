namespace Domain.Entities
{
    public class RatingEntity : BaseEntity
    {
        public string novel_id { get; set; }
        public string user_id { get; set; }
        public int score { get; set; }
        public string content { get; set; }
    }
}