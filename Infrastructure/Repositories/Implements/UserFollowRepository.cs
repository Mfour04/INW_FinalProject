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
            mongoDBHelper.CreateCollectionIfNotExistsAsync("user_follows").Wait();
            _collection = mongoDBHelper.GetCollection<UserFollowEntity>("user_follows");
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
        public async Task<bool> UnfollowAsync(string followerId, string followingId)
        {
            try
            {
                var result = await _collection.DeleteOneAsync(x => x.follower_id == followerId && x.following_id == followingId);
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
        public async Task<bool> IsFollowingAsync(string followerId, string followingId)
        {
            try
            {
                var count = await _collection.CountDocumentsAsync(x => x.follower_id == followerId && x.following_id == followingId);
                return count > 0;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        /// <summary>
        /// Lấy danh sách người mà user đang follow
        /// </summary>
        public async Task<List<UserFollowEntity>> GetFollowingAsync(string followerId)
        {
            try
            {
                return await _collection.Find(x => x.follower_id == followerId).ToListAsync();
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        /// <summary>
        /// Lấy danh sách người đang follow user
        /// </summary>
        public async Task<List<UserFollowEntity>> GetFollowersAsync(string followingId)
        {
            try
            {
                return await _collection.Find(x => x.following_id == followingId).ToListAsync();
            }
            catch
            {
                throw new InternalServerException();
            }
        }
    }
}