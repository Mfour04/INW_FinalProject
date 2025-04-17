using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.InwContext
{
    public class InwDataContext
    {
        private readonly IMongoDatabase _database;
        private readonly IMongoCollection<Users> _users;
        public InwDataContext(IOptions<MongoSetting> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            _database = client.GetDatabase(settings.Value.DatabaseName);
            _users = _database.GetCollection<Users>("Users");
            CreateIndexes();
        }

        public IMongoCollection<Users> Users => _users;
        public IMongoCollection<T> GetCollection<T>(string collectionName)
        {
            return _database.GetCollection<T>(collectionName);
        }
        private void CreateIndexes()
        {
            var indexKeys = Builders<Users>.IndexKeys.Ascending(u => u.Email); // tạo index trên Email
            var indexOptions = new CreateIndexOptions { Unique = true }; // đảm bảo duy nhất

            var indexModel = new CreateIndexModel<Users>(indexKeys, indexOptions);

            _users.Indexes.CreateOne(indexModel); // tạo index trên collection
        }
    }
}
