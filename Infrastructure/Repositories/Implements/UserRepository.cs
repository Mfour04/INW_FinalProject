using Domain.Entities;
using Infrastructure.InwContext;
using Infrastructure.Repositories.Interfaces;
using MongoDB.Driver;
using Shared.Exceptions;
using Shared.Helpers;

namespace Infrastructure.Repositories.Implements
{
    public class UserRepository : IUserRepository
    {
        private readonly IMongoCollection<UserEntity> _collection;

        public UserRepository(MongoDBHelper mongoDBHelper)
        {
            // Tạo collection nếu chưa có
            mongoDBHelper.CreateCollectionIfNotExistsAsync("user").Wait();

            // Gán collection
            _collection = mongoDBHelper.GetCollection<UserEntity>("user");
        }

        public async Task<UserEntity> CreateUser(UserEntity entity)
        {
            try
            {
				entity.displayname_normalized = SystemHelper.RemoveDiacritics(entity.displayname);
				await _collection.InsertOneAsync(entity);

                return entity;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<UserEntity> GetByEmail(string email)
        {
            try
            {
                var result = await _collection.Find(x => x.email == email).FirstOrDefaultAsync();
                return result;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<UserEntity> GetById(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("userId must not be null or empty");
            try
            {
                var result = await _collection.Find(x => x.id == userId).FirstOrDefaultAsync();
                return result;
            }
            catch (Exception ex)
            {
                throw new InternalServerException("Error getting user by id: " + ex.Message);
            }
        }

        public async Task<UserEntity> GetByName(string userName)
        {
             try
            {
                var result = await _collection.Find(x => x.username == userName).FirstOrDefaultAsync();
                return result;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<UserEntity> UpdateUser(UserEntity entity)
        {
            try
            {
                var filter = Builders<UserEntity>.Filter.Eq(x => x.id, entity.id);
                var result = await _collection.ReplaceOneAsync(filter, entity);

                return entity;
            }
            catch
            {
                throw new InternalServerException();
            }
        }
    }
}
