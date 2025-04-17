using Infrastructure.InwContext;
using Infrastructure.Repositories.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.Implements
{
    public class BaseRepository<T> : IBaseRepository<T> where T : class

    {
        private readonly InwDataContext _context;
        private readonly IMongoCollection<T> _collection;

        public BaseRepository(InwDataContext context, string collectionName)
        {
            _context = context;
            _collection = context.GetCollection<T>(collectionName); 

        }

        public async Task<T> AddAsync(T entity)
        {
            await _collection.InsertOneAsync(entity);
            return entity;
        }

        public async Task<T> UpdateAsync(T entity)
        {
            // Lấy ID từ entity (giả sử entity có trường _id kiểu ObjectId)
            var idProperty = typeof(T).GetProperty("_id");
            var id = idProperty.GetValue(entity);

            var result = await _collection.ReplaceOneAsync(
                Builders<T>.Filter.Eq("_id", id),
                entity,
                new ReplaceOptions { IsUpsert = true });

            if (result.ModifiedCount > 0)
                return entity;

            return default;  // Nếu không có gì được sửa
        }

        public async Task<bool> DeleteAsync(ObjectId id)
        {
            var result = await _collection.DeleteOneAsync(Builders<T>.Filter.Eq("_id", id));
            return result.DeletedCount > 0;
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _collection.Find(Builders<T>.Filter.Empty).ToListAsync();
        }

        public async Task<T> GetByIdAsync(ObjectId id)
        {
            return await _collection.Find(Builders<T>.Filter.Eq("_id", id)).FirstOrDefaultAsync();
        }
    }
}
