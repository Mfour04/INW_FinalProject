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

        /// <summary>
        /// Tạo mới một bình luận
        /// </summary>
        public async Task<CommentEntity> CreateAsync(CommentEntity entity)
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
        /// Cập nhật nội dung bình luận
        /// </summary>
        public async Task<bool> UpdateAsync(string id, CommentEntity entity)
        {
            try
            {
                var filter = Builders<CommentEntity>.Filter.Eq(x => x.id, id);

                var comment = await _collection.Find(filter).FirstOrDefaultAsync();

                var update = Builders<CommentEntity>
                    .Update.Set(x => x.content, entity.content ?? comment.content)
                    .Set(x => x.updated_at, TimeHelper.NowTicks);

                var updated = await _collection.FindOneAndUpdateAsync(
                    filter,
                    update,
                    new FindOneAndUpdateOptions<CommentEntity>
                    {
                        ReturnDocument = ReturnDocument.After,
                    }
                );

                return updated != null;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        /// <summary>
        /// Xóa một bình luận
        /// </summary>
        public async Task<bool> DeleteAsync(string id)
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

        /// <summary>
        /// Xóa tất cả bình luận con theo parentId
        /// </summary>
        public async Task<bool> DeleteRepliesByParentIdAsync(string parentId)
        {
            try
            {
                var filter = Builders<CommentEntity>.Filter.Eq(c => c.parent_comment_id, parentId);
                var deleted = await _collection.DeleteManyAsync(filter);

                return deleted.DeletedCount > 0;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        /// <summary>
        /// Lấy thông tin một bình luận theo Id
        /// </summary>
        public async Task<CommentEntity> GetByIdAsync(string commentId)
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

        /// <summary>
        /// Lấy danh sách bình luận của Novel
        /// </summary>
        public async Task<List<CommentEntity>> GetCommentsByNovelIdAsync(string novelId, FindCreterias creterias, List<SortCreterias> sortCreterias)
        {
            try
            {
                var builder = Builders<CommentEntity>.Filter;
                var filtered = builder.And(
                    builder.Eq(x => x.novel_id, novelId),
                    builder.Eq(x => x.chapter_id, null),
                    builder.Eq(x => x.parent_comment_id, null)
                );

                var query = _collection
                  .Find(filtered)
                  .Skip(creterias.Page * creterias.Limit)
                  .Limit(creterias.Limit);

                var sortBuilder = Builders<CommentEntity>.Sort;
                var sortDefinitions = new List<SortDefinition<CommentEntity>>();

                foreach (var criterion in sortCreterias)
                {
                    SortDefinition<CommentEntity>? sortDef = criterion.Field switch
                    {
                        "created_at" => criterion.IsDescending
                            ? sortBuilder.Descending(x => x.created_at)
                            : sortBuilder.Ascending(x => x.created_at),
                        _ => null
                    };

                    if (sortDef != null)
                        sortDefinitions.Add(sortDef);
                }

                if (sortDefinitions.Count >= 1)
                {
                    var combinedSort = sortBuilder.Combine(sortDefinitions);
                    query = query.Sort(combinedSort);
                }

                return await query.ToListAsync();
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        /// <summary>
        /// Lấy danh sách bình luận của Chapter
        /// </summary>
        public async Task<List<CommentEntity>> GetCommentsByChapterIdAsync(string novelId, string chapterId, FindCreterias creterias, List<SortCreterias> sortCreterias)
        {
            try
            {
                var builder = Builders<CommentEntity>.Filter;
                var filtered = builder.And(
                    builder.Eq(x => x.novel_id, novelId),
                    builder.Eq(x => x.chapter_id, chapterId),
                    builder.Eq(x => x.parent_comment_id, null)
                );

                var query = _collection
                  .Find(filtered)
                  .Skip(creterias.Page * creterias.Limit)
                  .Limit(creterias.Limit);

                var sortBuilder = Builders<CommentEntity>.Sort;
                var sortDefinitions = new List<SortDefinition<CommentEntity>>();

                foreach (var criterion in sortCreterias)
                {
                    SortDefinition<CommentEntity>? sortDef = criterion.Field switch
                    {
                        "created_at" => criterion.IsDescending
                            ? sortBuilder.Descending(x => x.created_at)
                            : sortBuilder.Ascending(x => x.created_at),
                        _ => null
                    };

                    if (sortDef != null)
                        sortDefinitions.Add(sortDef);
                }

                if (sortDefinitions.Count >= 1)
                {
                    var combinedSort = sortBuilder.Combine(sortDefinitions);
                    query = query.Sort(combinedSort);
                }

                return await query.ToListAsync();
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        /// <summary>
        /// Lấy danh sách bình luận con
        /// </summary>
        public async Task<List<CommentEntity>> GetRepliesByCommentIdAsync(string parentId, FindCreterias creterias, List<SortCreterias> sortCreterias)
        {
            try
            {
                var builder = Builders<CommentEntity>.Filter;
                var filtered = builder.And(builder.Eq(x => x.parent_comment_id, parentId));

                var query = _collection
                  .Find(filtered)
                  .Skip(creterias.Page * creterias.Limit)
                  .Limit(creterias.Limit);

                var sortBuilder = Builders<CommentEntity>.Sort;
                var sortDefinitions = new List<SortDefinition<CommentEntity>>();

                foreach (var criterion in sortCreterias)
                {
                    SortDefinition<CommentEntity>? sortDef = criterion.Field switch
                    {
                        "created_at" => criterion.IsDescending
                            ? sortBuilder.Descending(x => x.created_at)
                            : sortBuilder.Ascending(x => x.created_at),
                        _ => null
                    };

                    if (sortDef != null)
                        sortDefinitions.Add(sortDef);
                }

                if (sortDefinitions.Count >= 1)
                {
                    var combinedSort = sortBuilder.Combine(sortDefinitions);
                    query = query.Sort(combinedSort);
                }

                return await query.ToListAsync();
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<List<string>> GetReplyIdsByParentIdAsync(string parentId)
        {
            try
            {
                var filter = Builders<CommentEntity>.Filter.Eq(c => c.parent_comment_id, parentId);
                var projection = Builders<CommentEntity>.Projection.Include(c => c.id);

                var replies = await _collection.Find(filter)
                                               .Project<CommentEntity>(projection)
                                               .ToListAsync();

                return replies.Select(r => r.id).ToList();
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        /// <summary>
        /// Kiểm tra bình luận trùng trong khoảng thời gian nhất định
        /// </summary>
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

        /// <summary>
        /// Kiểm tra người dùng có spam bình luận trong thời gian ngắn hay không
        /// </summary>
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

        /// <summary>
        /// Đếm số lượng reply của mỗi comment
        /// Nếu hệ thống cần mở rộng thì phải dùng <<Aggregation Pipeline>>
        /// </summary>
        public async Task<Dictionary<string, int>> CountRepliesPerCommentAsync(List<string> parentCommentIds)
        {
            try
            {
                var filter = Builders<CommentEntity>.Filter.In(x => x.parent_comment_id, parentCommentIds);

                var replies = await _collection.Find(filter).ToListAsync();

                var replyCounts = replies
                    .GroupBy(r => r.parent_comment_id)
                    .ToDictionary(g => g.Key!, g => g.Count());

                return replyCounts;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task DeleteManyAsync(List<string> ids)
        {
            try
            {
                var filter = Builders<CommentEntity>.Filter.In(c => c.id, ids);
                await _collection.DeleteManyAsync(filter);
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<bool> IncreaseLikeCountAsync(string commentId, int inc = 1)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(commentId)) return false;

                var filter = Builders<CommentEntity>.Filter.Eq(x => x.id, commentId);
                var update = Builders<CommentEntity>.Update.Inc(x => x.like_count, inc);

                var result = await _collection.UpdateOneAsync(filter, update);
                return result.ModifiedCount > 0;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<bool> DecreaseLikeCountAsync(string commentId, int dec = 1)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(commentId)) return false;

                var filter = Builders<CommentEntity>.Filter.And(
                    Builders<CommentEntity>.Filter.Eq(x => x.id, commentId),
                    Builders<CommentEntity>.Filter.Gt(x => x.like_count, 0) 
                );
                var update = Builders<CommentEntity>.Update.Inc(x => x.like_count, -dec);

                var result = await _collection.UpdateOneAsync(filter, update);
                return result.ModifiedCount > 0;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task DeleteChapterCommentsAsync(string chapterId)
        {
            var filter = Builders<CommentEntity>.Filter.Eq(c => c.chapter_id, chapterId);
            await _collection.DeleteManyAsync(filter);
        }
    }
}
