using Domain.Entities;
using Domain.Enums;
using Infrastructure.InwContext;
using Infrastructure.Repositories.Interfaces;
using MongoDB.Driver;
using Shared.Exceptions;
using Shared.Helpers;
using SharpCompress.Common;

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

        public async Task IncreaseCoinAsync(string userId, int amount)
        {
            try
            {
                var filter = Builders<UserEntity>.Filter.Eq(u => u.id, userId);
                var update = Builders<UserEntity>.Update.Inc(u => u.coin, amount);

                var result = await _collection.UpdateOneAsync(filter, update);
            }
            catch
            {
                throw new InternalServerException();

            }
        }

        public async Task<bool> DecreaseCoinAsync(string userId, int amount)
        {
            try
            {
                var filter = Builders<UserEntity>.Filter.And(
                    Builders<UserEntity>.Filter.Eq(u => u.id, userId),
                    Builders<UserEntity>.Filter.Gte(u => u.coin, amount) 
                );

                var update = Builders<UserEntity>.Update.Inc(u => u.coin, -amount);

                var result = await _collection.UpdateOneAsync(filter, update);

                return result.ModifiedCount > 0; 
            }
            catch (Exception)
            {
                throw new InternalServerException();
            }
        }

        public async Task<bool> UpdateUserRoleToAdminAsync(string userId)
        {
            var filter = Builders<UserEntity>.Filter.Eq(u => u.id, userId);
            var update = Builders<UserEntity>.Update.Set(nameof(UserEntity.role), Role.Admin);

            var result = await _collection.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }
        public async Task<UserEntity?> GetFirstUserByRoleAsync(Role role)
        {
            var filter = Builders<UserEntity>.Filter.Eq(nameof(UserEntity.role), role);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<UserEntity> GetUserNameByUserId(string userId)
        {
            try
            {
                var filter = Builders<UserEntity>.Filter.Eq(x => x.id, userId);
                var projection = Builders<UserEntity>.Projection.Include(x => x.username);
                var result = await _collection.Find(filter).Project<UserEntity>(projection).FirstOrDefaultAsync();

                return result;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<List<UserEntity>> GetUsersByIdsAsync(List<string> userIds)
        {
            try
            {
                var filter = Builders<UserEntity>.Filter.In(u => u.id, userIds);
                var result = await _collection.Find(filter).ToListAsync();
                return result;
            }
            catch
            {
                throw new InternalServerException();
            }
        }
    }
}
