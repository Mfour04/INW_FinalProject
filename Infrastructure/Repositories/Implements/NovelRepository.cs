using Domain.Entities;
using Domain.Entities.System;
using Infrastructure.InwContext;
using Infrastructure.Repositories.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;
using Shared.Contracts.Response.Admin;
using Shared.Exceptions;
using Shared.Helpers;
using System.Linq.Expressions;

namespace Infrastructure.Repositories.Implements
{
    public class NovelRepository : INovelRepository
    {
        private readonly IMongoCollection<NovelEntity> _collection;
        private readonly IChapterRepository _chapterRepository;
        private readonly ITagRepository _tagRepository;
        public NovelRepository(MongoDBHelper mongoDBHelper, IChapterRepository chapterRepository, ITagRepository tagRepository)
        {
            mongoDBHelper.CreateCollectionIfNotExistsAsync("novel").Wait();
            _collection = mongoDBHelper.GetCollection<NovelEntity>("novel");
            _chapterRepository = chapterRepository;
            _tagRepository = tagRepository;
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

        public async Task<(List<NovelEntity> Novels, int TotalCount)> GetAllNovelAsync(FindCreterias creterias, List<SortCreterias> sortCreterias)
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
                // 📌 Filter theo tag name
                if (creterias.SearchTagTerm != null && creterias.SearchTagTerm.Any())
                {
                    var matchedTags = await _tagRepository.GetByNamesAsync(creterias.SearchTagTerm);
                    var tagIds = matchedTags.Select(t => t.id).ToList();

                    if (tagIds.Any())
                    {
                        filtered &= builder.In("tags", tagIds);
                    }
                    else
                    {
                        // Không có tag phù hợp → trả về rỗng
                        return (new List<NovelEntity>(), 0);
                    }
                }

                var query = _collection
                    .Find(filtered)
                    .Skip(creterias.Page * creterias.Limit)
                    .Limit(creterias.Limit);

                var sortBuilder = Builders<NovelEntity>.Sort;
                var sortDefinitions = new List<SortDefinition<NovelEntity>>();
                var totalCount = await _collection.CountDocumentsAsync(filtered);
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
                var novels = await query.ToListAsync();
                return (novels, (int)totalCount);
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

        public async Task<NovelEntity> GetBySlugAsync(string slugName)
        {
            try
            {
                var result = await _collection.Find(x => x.slug == slugName.Trim()).FirstOrDefaultAsync();
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
                int totalChapters = await _chapterRepository.CountPublishedAsync(novelId);

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

        public async Task<List<NovelEntity>> GetNovelByAuthorId(string authorId)
        {
            try
            {
                return await _collection.Find(x => x.author_id == authorId).ToListAsync();
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task UpdateLockStatusAsync(string novelId, bool isLocked)
        {
            try
            {
                var filter = Builders<NovelEntity>.Filter.Eq(x => x.id, novelId);
                var updateLock = Builders<NovelEntity>.Update.Combine(
                                 Builders<NovelEntity>.Update.Set(x => x.is_lock, isLocked),
                                 Builders<NovelEntity>.Update.Set(x => x.is_public, !isLocked));
                await _collection.UpdateOneAsync(filter, updateLock);
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<bool> IncrementCommentsAsync(string novelId)
        {
            try
            {
                var update = Builders<NovelEntity>.Update.Inc(x => x.comment_count, 1);
                var result = await _collection.UpdateOneAsync(x => x.id == novelId, update);

                return result.ModifiedCount > 0;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<bool> DecrementCommentsAsync(string novelId)
        {
            try
            {
                var update = Builders<NovelEntity>.Update.Inc(x => x.comment_count, -1);
                var result = await _collection.UpdateOneAsync(x => x.id == novelId, update);

                return result.ModifiedCount > 0;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task UpdateHideNovelAsync(string novelId, bool isPublic)
        {
            try
            {
                var filter = Builders<NovelEntity>.Filter.Eq(x => x.id, novelId);
                var updatehide = Builders<NovelEntity>.Update.Set(x => x.is_public, isPublic);
                await _collection.UpdateOneAsync(filter, updatehide);
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<bool> IsSlugExistsAsync(string slug, string? excludeId = null)
        {
            try
            {
                var filter = Builders<NovelEntity>.Filter.Eq(n => n.slug, slug);
                if (!string.IsNullOrEmpty(excludeId))
                {
                    filter &= Builders<NovelEntity>.Filter.Ne(n => n.id, excludeId);
                }

                var result = await _collection.Find(filter).AnyAsync();
                return result;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<List<NovelEntity>> GetManyByIdsAsync(List<string> ids)
        {
            try
            {
                var filter = Builders<NovelEntity>.Filter.In(x => x.id, ids);
                return await _collection.Find(filter).ToListAsync();
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<int> CountAsync(Expression<Func<NovelEntity, bool>> filter = null)
        {
            return (int)(filter == null
            ? await _collection.CountDocumentsAsync(_ => true)
            : await _collection.CountDocumentsAsync(filter));
        }

        public async Task<List<WeeklyStatItem>> CountNovelsPerDayCurrentWeekAsync()
        {
            var fromTicks = TimeHelper.StartOfCurrentWeekTicksVN;
            var toTicks = TimeHelper.NowTicks;

            var novels = await _collection
                .Find(n => n.created_at >= fromTicks && n.created_at <= toTicks)
                .ToListAsync();

            // Nhóm theo ngày (VN timezone)
            var grouped = novels
                .GroupBy(n => TimeHelper.ToVN(new DateTime(n.created_at)).Date)
                .ToDictionary(g => g.Key, g => g.Count());

            // Danh sách các ngày từ thứ 2 đến hôm nay
            var days = TimeHelper.GetDaysFromStartOfWeekToTodayVN();

            var result = days.Select(d => new WeeklyStatItem
            {
                Day = d.ToString("yyyy-MM-dd"),
                Count = grouped.ContainsKey(d) ? grouped[d] : 0,
                Weekday = TimeHelper.DayOfWeekVN(d.DayOfWeek)
            }).ToList();

            return result;
        }
    }
}
