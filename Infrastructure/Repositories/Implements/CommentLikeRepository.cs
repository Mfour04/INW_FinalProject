using Domain.Entities;
using Infrastructure.InwContext;
using Infrastructure.Repositories.Interfaces;
using MongoDB.Driver;
using Shared.Exceptions;

namespace Infrastructure.Repositories.Implements
{
    public class CommentLikeRepository : ICommentLikeRepository
    {
        private readonly IMongoCollection<CommentLikeEntity> _collection;

        public CommentLikeRepository(MongoDBHelper mongoDBHelper)
        {
            mongoDBHelper.CreateCollectionIfNotExistsAsync("comment_like").Wait();
            _collection = mongoDBHelper.GetCollection<CommentLikeEntity>("comment_like");
        }

        public async Task<CommentLikeEntity> LikeCommentAsync(CommentLikeEntity entity)
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

        public async Task<bool> UnlikeCommentAsync(string postId, string userId)
        {
            try
            {
                var filter = Builders<CommentLikeEntity>.Filter.Eq(x => x.comment_id, postId) &
                Builders<CommentLikeEntity>.Filter.Eq(x => x.user_id, userId);

                var deleted = await _collection.FindOneAndDeleteAsync(filter);

                return deleted != null;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<bool> HasUserLikedCommentAsync(string postId, string userId)
        {
            var filter = Builders<CommentLikeEntity>.Filter.Eq(x => x.comment_id, postId) &
                         Builders<CommentLikeEntity>.Filter.Eq(x => x.user_id, userId);

            return await _collection.Find(filter).AnyAsync();
        }
    }
}