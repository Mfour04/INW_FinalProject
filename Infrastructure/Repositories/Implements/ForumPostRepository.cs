using Domain.Entities;
using Domain.Entities.System;
using Infrastructure.InwContext;
using Infrastructure.Repositories.Interfaces;
using MongoDB.Driver;
using Shared.Exceptions;

namespace Infrastructure.Repositories.Implements
{
    public class ForumPostRepository : IForumPostRepository
    {
        private readonly IMongoCollection<ForumPostEntity> _collection;

        public ForumPostRepository(MongoDBHelper mongoDBHelper)
        {
            mongoDBHelper.CreateCollectionIfNotExistsAsync("forum_post").Wait();
            _collection = mongoDBHelper.GetCollection<ForumPostEntity>("forum_post");
        }

        public async Task<List<ForumPostEntity>> GetAllAsync(FindCreterias creterias, List<SortCreterias> sortCreterias)
        {
            try
            {
                var builder = Builders<ForumPostEntity>.Filter;
                var filtered = builder.Empty;

                var query = _collection
                    .Find(filtered)
                    .Skip(creterias.Page * creterias.Limit)
                    .Limit(creterias.Limit);

                var sortBuilder = Builders<ForumPostEntity>.Sort;
                var sortDefinitions = new List<SortDefinition<ForumPostEntity>>();

                foreach (var criterion in sortCreterias)
                {
                    SortDefinition<ForumPostEntity>? sortDef = criterion.Field switch
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

        public async Task<ForumPostEntity> GetByIdAsync(string id)
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

        public async Task<ForumPostEntity> CreateAsync(ForumPostEntity entity)
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

        public async Task<bool> UpdateAsync(string id, ForumPostEntity entity)
        {
            try
            {
                var filter = Builders<ForumPostEntity>.Filter.Eq(x => x.id, id);

                var post = await _collection.Find(filter).FirstOrDefaultAsync();

                var update = Builders<ForumPostEntity>
                    .Update.Set(x => x.content, entity.content ?? post.content)
                    .Set(x => x.updated_at, DateTime.Now.Ticks);

                var updated = await _collection.FindOneAndUpdateAsync(
                    filter,
                    update,
                    new FindOneAndUpdateOptions<ForumPostEntity>
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
                var filter = Builders<ForumPostEntity>.Filter.Eq(x => x.id, id);
                var deleted = await _collection.FindOneAndDeleteAsync(filter);

                return deleted != null;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<bool> IncrementLikesAsync(string id)
        {
            var update = Builders<ForumPostEntity>.Update.Inc(x => x.like_count, 1);
            var result = await _collection.UpdateOneAsync(
                x => x.id == id,
                update
            );
            return result.ModifiedCount > 0;
        }

        public async Task<bool> DecrementLikesAsync(string id)
        {
            var update = Builders<ForumPostEntity>.Update.Inc(x => x.like_count, -1);
            var result = await _collection.UpdateOneAsync(
                x => x.id == id,
                update
            );
            return result.ModifiedCount > 0;
        }

        public async Task<bool> IncrementCommentsAsync(string id)
        {
            try
            {
                var update = Builders<ForumPostEntity>.Update.Inc(x => x.comment_count, 1);
                var result = await _collection.UpdateOneAsync(x => x.id == id, update);

                return result.ModifiedCount > 0;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<bool> DecrementCommentsAsync(string id)
        {
            try
            {
                var update = Builders<ForumPostEntity>.Update.Inc(x => x.comment_count, -1);
                var result = await _collection.UpdateOneAsync(x => x.id == id, update);

                return result.ModifiedCount > 0;
            }
            catch
            {
                throw new InternalServerException();
            }
        }
    }
}