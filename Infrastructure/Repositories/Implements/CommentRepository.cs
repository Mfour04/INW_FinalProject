using Domain.Entities;
using Domain.Entities.System;
using Infrastructure.InwContext;
using Infrastructure.Repositories.Interfaces;
using MongoDB.Driver;
using Shared.Exceptions;
using Shared.Helpers;

namespace Infrastructure.Repositories.Implements
{
    public class CommentRepository : ICommentRepository
    {
        private readonly IMongoCollection<CommentEntity> _collection;
        public CommentRepository(MongoDBHelper mongoDBHelper)
        {
            mongoDBHelper.CreateCollectionIfNotExistsAsync("comment").Wait();
            _collection = mongoDBHelper.GetCollection<CommentEntity>("comment");
        }
        public async Task<CommentEntity> CreateCommentAsync(CommentEntity entity)
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

        public async Task<List<CommentEntity>> GetCommentsByNovelIdAndChapterIdAsync(FindCreterias findCreterias, string novelId, string chapterId = null)
        {
            try
            {
                var filtered = Builders<CommentEntity>.Filter.Eq(x => x.novel_id, novelId);

                if (!string.IsNullOrEmpty(chapterId))
                {
                    var chapterFilter = Builders<CommentEntity>.Filter.Eq(c => c.chapter_id, chapterId);
                    filtered = Builders<CommentEntity>.Filter.And(filtered, chapterFilter);
                }

                var query = _collection
                    .Find(filtered)
                    .Skip(findCreterias.Page * findCreterias.Limit)
                    .Limit(findCreterias.Limit);

                return await query.ToListAsync();
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<CommentEntity> GetCommentByIdAsync(string commentId)
        {
            try
            {
                var result = await _collection.Find(x => x.id == commentId).FirstOrDefaultAsync();
                return result;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<CommentEntity> UpdateCommentAsync(CommentEntity entity)
        {
            try
            {
                entity.updated_at = DateTime.UtcNow.Ticks;
                var filter = Builders<CommentEntity>.Filter.Eq(x => x.id, entity.id);
                var result = await _collection.ReplaceOneAsync(filter, entity);
                return entity;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<bool> DeleteCommentAsync(string id)
        {
            try
            {
                var filter = Builders<CommentEntity>.Filter.Eq(x => x.id, id);
                var deleted = await _collection.FindOneAndDeleteAsync(filter);

                return deleted != null;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<List<CommentEntity>> GetCommentsByNovelIdAsync(FindCreterias findCreterias, string novelId)
        {
            try
            {
                var filter = Builders<CommentEntity>.Filter.Eq(x => x.novel_id, novelId);
                var result = _collection
                    .Find(filter)
                    .SortByDescending(x => x.created_at)
                    .Skip(findCreterias.Page * findCreterias.Limit)
                    .Limit(findCreterias.Limit)
                    .ToListAsync();

                return await result;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<List<CommentEntity>> GetCommentsByChapterIdAsync(FindCreterias findCreterias, string chapterId)
        {
            try
            {
                var filter = Builders<CommentEntity>.Filter.Eq(x => x.chapter_id, chapterId);
                var result = _collection
                    .Find(filter)
                    .SortByDescending(x => x.created_at)
                    .Skip(findCreterias.Page * findCreterias.Limit)
                    .Limit(findCreterias.Limit)
                    .ToListAsync();
                return await result;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<List<CommentEntity>> GetRepliesByParentIdAsync(string parentCommentId)
        {
            try
            {
                var filter = Builders<CommentEntity>.Filter.Eq(x => x.parent_comment_id, parentCommentId);
                var result = _collection
                    .Find(filter)
                    .SortByDescending(x => x.created_at)
                    .ToListAsync();
                return await result;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<bool> IsDuplicateCommentAsync(string userId, string novelId, string? chapterId, string content, int withinMinutes)
        {
            try
            {
                var hash = SystemHelper.ComputeSha256(content.Trim().ToLower());
                var since = TimeHelper.NowVN.AddMinutes(-withinMinutes).Ticks;

				var filterBuilder = Builders<CommentEntity>.Filter;
                var filters = new List<FilterDefinition<CommentEntity>>
                    {
                        filterBuilder.Eq(x => x.user_id, userId),
                        filterBuilder.Eq(x => x.novel_id, novelId),
                        filterBuilder.Eq(x => x.content_hash, hash),
                        filterBuilder.Gte(x => x.created_at, since)
                    };

                if (!string.IsNullOrEmpty(chapterId))
                {
                    filters.Add(filterBuilder.Eq(x => x.chapter_id, chapterId));
                }
                else
                {
                    filters.Add(filterBuilder.Eq(x => x.chapter_id, null));
                }

                var filter = filterBuilder.And(filters);

                return await _collection.Find(filter).AnyAsync();
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<bool> IsSpammingTooFrequentlyAsync(string userId, int limit, int withinMinutes)
        {
            try
            {
                var since = TimeHelper.NowVN.AddMinutes(-withinMinutes).Ticks;

				var filter = Builders<CommentEntity>.Filter.And(
                    Builders<CommentEntity>.Filter.Eq(x => x.user_id, userId),
                    Builders<CommentEntity>.Filter.Gte(x => x.created_at, since)
                );

                var count = await _collection.CountDocumentsAsync(filter);
                return count >= limit;
            }
            catch
            {
                throw new InternalServerException();
            }
        }
    }
}
