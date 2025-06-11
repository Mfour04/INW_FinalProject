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

        public async Task<List<ForumPostEntity>> GetAllForumPostAsync(FindCreterias creterias, List<SortCreterias> sortCreterias)
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

        public async Task<ForumPostEntity> CreateForumPostAsync(ForumPostEntity entity)
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

        public async Task<bool> UpdateForumPostAsync(string id, ForumPostEntity entity)
        {
            try
            {
                // entity.updated_at = DateTime.UtcNow.Ticks;
                // var filter = Builders<ForumPostEntity>.Filter.Eq(x => x.id, entity.id);
                // var result = await _collection.ReplaceOneAsync(filter, entity);

                // return entity;

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

        public async Task<bool> DeleteForumPostAsync(string id)
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
    }
}