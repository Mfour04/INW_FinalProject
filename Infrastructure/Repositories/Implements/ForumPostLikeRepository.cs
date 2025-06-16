using Domain.Entities;
using Infrastructure.InwContext;
using Infrastructure.Repositories.Interfaces;
using MongoDB.Driver;
using Shared.Exceptions;

namespace Infrastructure.Repositories.Implements
{
    public class ForumPostLikeRepository : IForumPostLikeRepository
    {
        private readonly IMongoCollection<ForumPostLikeEntity> _collection;

        public ForumPostLikeRepository(MongoDBHelper mongoDBHelper)
        {
            mongoDBHelper.CreateCollectionIfNotExistsAsync("forum_post_like").Wait();
            _collection = mongoDBHelper.GetCollection<ForumPostLikeEntity>("forum_post_like");
        }

        public async Task<ForumPostLikeEntity> LikePostAsync(ForumPostLikeEntity entity)
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

        public async Task<bool> UnlikePostAsync(string postId, string userId)
        {
            try
            {
                var filter = Builders<ForumPostLikeEntity>.Filter.Eq(x => x.post_id, postId) &
                Builders<ForumPostLikeEntity>.Filter.Eq(x => x.user_id, userId);

                var deleted = await _collection.FindOneAndDeleteAsync(filter);

                return deleted != null;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<bool> HasUserLikedPostAsync(string postId, string userId)
        {
            var filter = Builders<ForumPostLikeEntity>.Filter.Eq(x => x.post_id, postId) &
                         Builders<ForumPostLikeEntity>.Filter.Eq(x => x.user_id, userId);

            return await _collection.Find(filter).AnyAsync();
        }
    }
}