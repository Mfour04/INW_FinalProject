namespace Infrastructure.InwContext
{
    public class MongoSetting
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }

        public MongoSetting()
        {
            // ConnectionString = "mongodb+srv://inwk_share:Inwk%402025@cluster-1.zwy5uec.mongodb.net/?retryWrites=true&w=majority&appName=Cluster-1";
            // DatabaseName = "INWK_Final_Project";

            ConnectionString = "mongodb://localhost:27017";
            DatabaseName = "INWProject";
        }
    }
}
