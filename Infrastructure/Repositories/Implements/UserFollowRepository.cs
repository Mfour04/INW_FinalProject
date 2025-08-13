using Domain.Entities;
using Infrastructure.InwContext;
using Infrastructure.Repositories.Interfaces;
using MongoDB.Driver;
using Shared.Exceptions;

namespace Infrastructure.Repositories.Implements
{
    public class UserFollowRepository : IUserFollowRepository
    {
        private readonly IMongoCollection<UserFollowEntity> _collection;

        public UserFollowRepository(MongoDBHelper mongoDBHelper)
        {
            mongoDBHelper.CreateCollectionIfNotExistsAsync("user_follow").Wait();
            _collection = mongoDBHelper.GetCollection<UserFollowEntity>("user_follow");
        }

        /// <summary>
        /// Tạo bản ghi follow mới
        /// </summary>
        public async Task<UserFollowEntity> FollowAsync(UserFollowEntity entity)
        {
            try
            {
                await _collection.InsertOneAsync(entity);
                return entity;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        /// <summary>
        /// Bỏ follow giữa 2 người dùng
        /// </summary>
        public async Task<bool> UnfollowAsync(string actorId, string targetId)
        {
            try
            {
                var result = await _collection.DeleteOneAsync(x => x.actor_id == actorId && x.target_id == targetId);
                return result.DeletedCount > 0;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        /// <summary>
        /// Kiểm tra xem một người dùng có đang theo dõi người dùng khác hay không
        /// </summary>
        public async Task<bool> IsFollowingAsync(string actorId, string targetId)
        {
            try
            {
                var count = await _collection.CountDocumentsAsync(x => x.actor_id == actorId && x.target_id == targetId);
                return count > 0;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        /// <summary>
        /// Lấy danh sách ID của người mà user đang follow
        /// </summary>
        public async Task<List<string>> GetFollowingIdsOfUserAsync(string userId)
        {
            try
            {
                var filter = Builders<UserFollowEntity>.Filter.Eq(x => x.actor_id, userId);
                var result = await _collection.Find(filter)
                    .Project(x => x.target_id)
                    .ToListAsync();

                return result;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        /// <summary>
        /// Lấy danh sách người đang follow user
        /// </summary>
        public async Task<List<string>> GetFollowerIdsOfUserAsync(string userId)
        {
            try
            {
                var filter = Builders<UserFollowEntity>.Filter.Eq(x => x.target_id, userId);
                var result = await _collection.Find(filter)
                    .Project(x => x.actor_id)
                    .ToListAsync();

                return result;
            }
            catch
            {
                throw new InternalServerException();
            }
        }
    }
}