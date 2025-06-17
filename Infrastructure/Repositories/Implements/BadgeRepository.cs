using Domain.Entities;
using Domain.Entities.System;
using Infrastructure.InwContext;
using Infrastructure.Repositories.Interfaces;
using MongoDB.Driver;
using Shared.Exceptions;

namespace Infrastructure.Repositories.Implements
{
    public class BadgeRepository : IBadgeRepository
    {
        private readonly IMongoCollection<BadgeEntity> _collection;

        public BadgeRepository(MongoDBHelper mongoDBHelper)
        {
            mongoDBHelper.CreateCollectionIfNotExistsAsync("badge").Wait();
            _collection = mongoDBHelper.GetCollection<BadgeEntity>("badge");
        }

        public async Task<List<BadgeEntity>> GetAllAsync(FindCreterias creterias, List<SortCreterias> sortCreterias)
        {
            try
            {
                var builder = Builders<BadgeEntity>.Filter;
                var filtered = builder.Empty;

                var query = _collection
                    .Find(filtered)
                    .Skip(creterias.Page * creterias.Limit)
                    .Limit(creterias.Limit);

                var sortBuilder = Builders<BadgeEntity>.Sort;
                var sortDefinitions = new List<SortDefinition<BadgeEntity>>();

                foreach (var criterion in sortCreterias)
                {
                    SortDefinition<BadgeEntity>? sortDef = criterion.Field switch
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

        public async Task<BadgeEntity> GetByIdAsync(string id)
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

        public async Task<BadgeEntity> CreateAsync(BadgeEntity entity)
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

        public async Task<bool> UpdateAsync(string id, BadgeEntity entity)
        {
            try
            {
                var filter = Builders<BadgeEntity>.Filter.Eq(x => x.id, id);

                var post = await _collection.Find(filter).FirstOrDefaultAsync();

                var update = Builders<BadgeEntity>
                    .Update.Set(x => x.name, entity.name ?? post.name)
                    .Set(x => x.description, entity.description ?? post.description)
                    .Set(x => x.icon_url, entity.icon_url ?? post.icon_url);

                var updated = await _collection.FindOneAndUpdateAsync(
                    filter,
                    update,
                    new FindOneAndUpdateOptions<BadgeEntity>
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
                var filter = Builders<BadgeEntity>.Filter.Eq(x => x.id, id);
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