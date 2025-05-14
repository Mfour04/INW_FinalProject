using MongoDB.Driver;

namespace Infrastructure.InwContext
{
    public class MongoDBHelper
    {
        private readonly IMongoDatabase _database;
        private readonly MongoClient _client;

        public MongoDBHelper()
        {
            var config = new MongoSetting();

            _client = new MongoClient(config.ConnectionString);
            _database = _client.GetDatabase(config.DatabaseName);
        }

        // Hàm generic để lấy bất kỳ collection nào
        public IMongoCollection<T> GetCollection<T>(string name) =>
            _database.GetCollection<T>(name);

        // Tạo collection nếu chưa có
        public async Task CreateCollectionIfNotExistsAsync(string name)
        {
            var collections = await _database.ListCollectionNames().ToListAsync();
            if (!collections.Contains(name))
            {
                await _database.CreateCollectionAsync(name);
            }
        }

        // Kiểm tra collection đã tồn tại chưa
        public async Task<bool> CollectionExistsAsync(string name)
        {
            var collections = await _database.ListCollectionNames().ToListAsync();
            return collections.Contains(name);
        }
    }
}
