using Domain.Entities;
using Domain.Entities.System;
using Infrastructure.InwContext;
using Infrastructure.Repositories.Interfaces;
using MongoDB.Driver;
using Shared.Exceptions;
using Shared.Helpers;

namespace Infrastructure.Repositories.Implements
{
    public class ForumCommentRepository : IForumCommentRepository
    {
        private readonly IMongoCollection<ForumCommentEntity> _collection;

        public ForumCommentRepository(MongoDBHelper mongoDBHelper)
        {
            mongoDBHelper.CreateCollectionIfNotExistsAsync("forum_comment").Wait();
            _collection = mongoDBHelper.GetCollection<ForumCommentEntity>("forum_comment");
        }

        public async Task<List<ForumCommentEntity>> GetRootCommentsByPostIdAsync(string postId, FindCreterias creterias, List<SortCreterias> sortCreterias)
        {
            try
            {
                var builder = Builders<ForumCommentEntity>.Filter;
                var filtered = builder.Eq(x => x.post_id, postId);

                var query = _collection
                    .Find(filtered)
                    .Skip(creterias.Page * creterias.Limit)
                    .Limit(creterias.Limit);

                var sortBuilder = Builders<ForumCommentEntity>.Sort;
                var sortDefinitions = new List<SortDefinition<ForumCommentEntity>>();

                foreach (var criterion in sortCreterias)
                {
                    SortDefinition<ForumCommentEntity>? sortDef = criterion.Field switch
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

        public async Task<List<ForumCommentEntity>> GetRepliesByCommentIdAsync(string parentId, FindCreterias creterias, List<SortCreterias> sortCreterias)
        {
            try
            {
                var builder = Builders<ForumCommentEntity>.Filter;
                var filter = builder.Eq(x => x.parent_comment_id, parentId);

                var query = _collection
                    .Find(filter)
                    .Skip(creterias.Page * creterias.Limit)
                    .Limit(creterias.Limit);

                var sortBuilder = Builders<ForumCommentEntity>.Sort;
                var sortDefinitions = new List<SortDefinition<ForumCommentEntity>>();

                foreach (var criterion in sortCreterias)
                {
                    SortDefinition<ForumCommentEntity>? sortDef = criterion.Field switch
                    {
                        "created_at" => criterion.IsDescending
                            ? sortBuilder.Descending(x => x.created_at)
                            : sortBuilder.Ascending(x => x.created_at),
                        _ => null
                    };

                    if (sortDef != null)
                        sortDefinitions.Add(sortDef);
                }

                if (sortDefinitions.Count > 0)
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

        public async Task<ForumCommentEntity> GetByIdAsync(string id)
        {
            try
            {
                var result = await _collection.Find(x => x.id == id).FirstOrDefaultAsync();
                return result;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<List<string>> GetReplyIdsByCommentIdAsync(string parentId)
        {
            try
            {
                var filter = Builders<ForumCommentEntity>.Filter.Eq(c => c.parent_comment_id, parentId);

                var projection = Builders<ForumCommentEntity>.Projection.Include(c => c.id);

                var result = await _collection.Find(filter)
                                .Project<ForumCommentEntity>(projection)
                                .ToListAsync();

                return result.Select(c => c.id).ToList();
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<ForumCommentEntity> CreateAsync(ForumCommentEntity entity)
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

        public async Task<bool> UpdateAsync(string id, ForumCommentEntity entity)
        {
            try
            {
                var filter = Builders<ForumCommentEntity>.Filter.Eq(x => x.id, id);

                var post = await _collection.Find(filter).FirstOrDefaultAsync();

                var update = Builders<ForumCommentEntity>
                    .Update.Set(x => x.content, entity.content ?? post.content)
                    .Set(x => x.updated_at, TimeHelper.NowTicks);

                var updated = await _collection.FindOneAndUpdateAsync(
                    filter,
                    update,
                    new FindOneAndUpdateOptions<ForumCommentEntity>
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

        public async Task<bool> DeleteAsync(string id)
        {
            try
            {
                var filter = Builders<ForumCommentEntity>.Filter.Eq(x => x.id, id);
                var deleted = await _collection.FindOneAndDeleteAsync(filter);

                return deleted != null;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<bool> DeleteManyAsync(List<string> ids)
        {
            try
            {
                var filter = Builders<ForumCommentEntity>.Filter.In(x => x.id, ids);
                var result = await _collection.DeleteManyAsync(filter);

                return result.DeletedCount > 0;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<bool> IncrementReplyCountAsync(string commentId)
        {
            try
            {
                var update = Builders<ForumCommentEntity>.Update.Inc(x => x.reply_count, 1);
                var result = await _collection.UpdateOneAsync(c => c.id == commentId, update);
                return result.ModifiedCount > 0;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task DeleteAllCommentByBlogId(string blogid)
        {
            var filter = Builders<ForumCommentEntity>.Filter.Eq(x => x.post_id, blogid);
            await _collection.DeleteManyAsync(filter);
        }
    }
}