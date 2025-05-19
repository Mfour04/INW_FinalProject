namespace Infrastructure.InwContext
{
    public class MongoSetting
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }

        public MongoSetting()
        {
            ConnectionString = "mongodb://localhost:27017";
            DatabaseName = "INWProject";
        }
    }
}
