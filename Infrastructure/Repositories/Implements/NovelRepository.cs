using Domain.Entities;
using Domain.Entities.System;
using Infrastructure.InwContext;
using Infrastructure.Repositories.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;
using Shared.Exceptions;
using ZstdSharp.Unsafe;

namespace Infrastructure.Repositories.Implements
{
    public class NovelRepository : INovelRepository
    {
        private readonly IMongoCollection<NovelEntity> _collection;
        public NovelRepository(MongoDBHelper mongoDBHelper)
        {
            mongoDBHelper.CreateCollectionIfNotExistsAsync("novel").Wait();
            _collection = mongoDBHelper.GetCollection<NovelEntity>("novel");
        }

        public async Task<NovelEntity> CreateNovelAsync(NovelEntity entity)
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

        public async Task DecrementFollowersAsync(string novelId)
        {
            var update = Builders<NovelEntity>.Update.Inc(x => x.followers, -1);
            await _collection.UpdateOneAsync(x => x.id == novelId, update);
        }

        public async Task<bool> DeleteNovelAsync(string id)
        {
            try
            {
                var filter = Builders<NovelEntity>.Filter.Eq(x => x.id, id);
                var deleted = await _collection.FindOneAndDeleteAsync(filter);

                return deleted != null;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<List<NovelEntity>> GetAllNovelAsync(FindCreterias creterias, List<SortCreterias> sortCreterias)
        {
            try
            {
                var builder = Builders<NovelEntity>.Filter;
                var filtered = builder.Empty;

                if (creterias.SearchTerm.Count >= 1)
                {
                    var regexFilters = creterias.SearchTerm.Select(keyword =>
                        builder.Regex(
                            x => x.title_unsigned,
                            new BsonRegularExpression(keyword, "i")
                        )
                    );
                    filtered &= builder.Or(regexFilters);
                }

                var query = _collection
                    .Find(filtered)
                    .Skip(creterias.Page * creterias.Limit)
                    .Limit(creterias.Limit);

                var sortBuilder = Builders<NovelEntity>.Sort;
                var sortDefinitions = new List<SortDefinition<NovelEntity>>();

                foreach (var criterion in sortCreterias)
                {
                    SortDefinition<NovelEntity>? sortDef = criterion.Field switch
                    {
                        "created_at" => criterion.IsDescending
                            ? sortBuilder.Descending(x => x.created_at)
                            : sortBuilder.Ascending(x => x.created_at),

                        "total_views" => criterion.IsDescending
                            ? sortBuilder.Descending(x => x.total_views)
                            : sortBuilder.Ascending(x => x.total_views),

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

        public async Task<NovelEntity> GetByNovelIdAsync(string novelId)
        {
            try
            {
                var result = await _collection.Find(x => x.id == novelId).FirstOrDefaultAsync();
                return result;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task IncrementFollowersAsync(string novelId)
        {
            var update = Builders<NovelEntity>.Update.Inc(x => x.followers, 1);
            await _collection.UpdateOneAsync(x => x.id == novelId, update);
        }

        public async Task<NovelEntity> UpdateNovelAsync(NovelEntity entity)
        {
           try
            {
                entity.updated_at = DateTime.UtcNow.Ticks;
                var filter = Builders<NovelEntity>.Filter.Eq(x => x.id, entity.id);
                var result = await _collection.ReplaceOneAsync(filter, entity);

                return entity;
            }
            catch
            {
                throw new InternalServerException();
            }
        }
    }
}
