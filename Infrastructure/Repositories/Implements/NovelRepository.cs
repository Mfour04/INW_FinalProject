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
        private IChapterRepository _chapterRepository;
        public NovelRepository(MongoDBHelper mongoDBHelper, IChapterRepository chapterRepository)
        {
            mongoDBHelper.CreateCollectionIfNotExistsAsync("novel").Wait();
            _collection = mongoDBHelper.GetCollection<NovelEntity>("novel");
            _chapterRepository = chapterRepository;
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
                var isExact = true;
                if (creterias.SearchTerm != null && creterias.SearchTerm.Count > 0)
                {
                    if (creterias.SearchTerm.Count == 1)
                    {
                        var keyword = creterias.SearchTerm[0];

                        // ✅ THAY builder.Eq thành Regex để chứa từ đó (fuzzy nhẹ)
                        filtered &= builder.Regex(
                            x => x.title_unsigned,
                            new BsonRegularExpression(keyword, "i")
                        );
                    }
                    else
                    {
                        // Fuzzy match: tất cả từ phải khớp
                        var regexFilters = creterias.SearchTerm.Select(term =>
                            builder.Regex(x => x.title_unsigned, new BsonRegularExpression(term, "i"))
                        );
                        filtered &= builder.And(regexFilters);
                    }
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
                        "rating_avg" => criterion.IsDescending
                            ? sortBuilder.Descending(x => x.rating_avg)
                            : sortBuilder.Ascending(x => x.rating_avg),

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

        public async Task IncreaseTotalViewAsync(string novelId)
        {
            var filtered = Builders<NovelEntity>.Filter.Eq(x => x.id, novelId);
            var update = Builders<NovelEntity>.Update.Inc(x => x.total_views, 1);
            await _collection.UpdateOneAsync(filtered, update);
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
                var filter = Builders<NovelEntity>.Filter.Eq(x => x.id, entity.id);
                var result = await _collection.ReplaceOneAsync(filter, entity);

                return entity;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task UpdateTotalChaptersAsync(string novelId)
        {
            try
            {
                int totalChapters = await _chapterRepository.GetTotalPublicChaptersAsync(novelId);

                var update = Builders<NovelEntity>.Update.Set(n => n.total_chapters, totalChapters);
                await _collection.UpdateOneAsync(
                    Builders<NovelEntity>.Filter.Eq(n => n.id, novelId),
                    update
                );
            }
            catch
            {
                throw new InternalServerException();
            }
        }
    }
}
