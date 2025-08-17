using Domain.Entities;
using Domain.Entities.System;
using Infrastructure.InwContext;
using Infrastructure.Repositories.Interfaces;
using MongoDB.Driver;
using Shared.Exceptions;
using Shared.Helpers;

namespace Infrastructure.Repositories.Implements
{
    public class RatingRepository : IRatingRepository
    {
        private readonly IMongoCollection<RatingEntity> _collection;

        public RatingRepository(MongoDBHelper mongoDBHelper)
        {
            mongoDBHelper.CreateCollectionIfNotExistsAsync("rating").Wait();
            _collection = mongoDBHelper.GetCollection<RatingEntity>("rating");
        }

        /// <summary>
        /// Lấy danh sách đánh giá theo novelId
        /// </summary>
        public async Task<List<RatingEntity>> GetByNovelIdAsync(string novelId, FindCreterias creterias, List<SortCreterias> sortCreterias)
        {
            try
            {
                var builder = Builders<RatingEntity>.Filter;
                var filtered = builder.And(builder.Eq(x => x.novel_id, novelId));

                var query = _collection
                  .Find(filtered)
                  .Skip(creterias.Page * creterias.Limit)
                  .Limit(creterias.Limit);

                var sortBuilder = Builders<RatingEntity>.Sort;
                var sortDefinitions = new List<SortDefinition<RatingEntity>>();

                foreach (var criterion in sortCreterias)
                {
                    SortDefinition<RatingEntity>? sortDef = criterion.Field switch
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
        /// Lấy thông tin chi tiết một đánh giá 
        /// </summary>
        public async Task<RatingEntity> GetByIdAsync(string id)
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

        public async Task<RatingEntity> CreateAsync(RatingEntity entity)
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
        /// Cập nhật thông tin đánh giá
        /// </summary>
        public async Task<bool> UpdateAsync(string id, RatingEntity entity)
        {
            try
            {
                var filter = Builders<RatingEntity>.Filter.Eq(x => x.id, id);

                var rating = await _collection.Find(filter).FirstOrDefaultAsync();

                var update = Builders<RatingEntity>
                    .Update.Set(x => x.score, entity?.score ?? rating.score)
                    .Set(x => x.content, entity.content ?? rating.content)
                    .Set(x => x.updated_at, TimeHelper.NowTicks);

                var updated = await _collection.FindOneAndUpdateAsync(
                    filter,
                    update,
                    new FindOneAndUpdateOptions<RatingEntity>
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
        /// Cập nhật thông tin đánh giá 
        /// </summary>
        public async Task<bool> DeleteAsync(string id)
        {
            try
            {
                var filter = Builders<RatingEntity>.Filter.Eq(x => x.id, id);
                var deleted = await _collection.FindOneAndDeleteAsync(filter);

                return deleted != null;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        /// <summary>
        /// Kiểm tra người dùng đã đánh giá truyện hay chưa
        /// </summary>
        public async Task<bool> HasUserRatedNovelAsync(string userId, string novelId)
        {
            try
            {
                var filter = Builders<RatingEntity>.Filter.And(
                    Builders<RatingEntity>.Filter.Eq(x => x.user_id, userId),
                    Builders<RatingEntity>.Filter.Eq(x => x.novel_id, novelId)
                );

                return await _collection.Find(filter).AnyAsync();
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        /// <summary>
        /// Tính trung bình điểm đánh giá của một truyện
        /// </summary>
        public async Task<double> GetAverageRatingByNovelIdAsync(string novelId)
        {
            try
            {
                var filter = Builders<RatingEntity>.Filter.Eq(r => r.novel_id, novelId);
                var ratings = await _collection.Find(filter).ToListAsync();

                if (!ratings.Any())
                    return 0;

                var average = ratings.Average(r => r.score);
                var result = Math.Round(average, 2);

                return result;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        /// <summary>
        /// Đếm số lượng đánh giá của một truyện 
        /// </summary>
        public async Task<int> GetRatingCountByNovelIdAsync(string novelId)
        {
            try
            {
                var filter = Builders<RatingEntity>.Filter.Eq(r => r.novel_id, novelId);
                var count = await _collection.CountDocumentsAsync(filter);

                var result = (int)count;
                return result;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

		public async Task<(IReadOnlyList<RatingEntity> items, bool hasMore)> GetByNovelIdKeysetAsync(
	        string novelId,
	        int limit,
	        long? afterCreatedAtTicks,
	        CancellationToken ct)
		{
			var query = _collection.AsQueryable()
				.Where(x => x.novel_id == novelId);

			if (afterCreatedAtTicks.HasValue && afterCreatedAtTicks.Value > 0)
			{
				query = query.Where(x => x.created_at < afterCreatedAtTicks.Value);
			}

			var list = query
				.OrderByDescending(x => x.created_at)
				.Take(limit + 1)
				.ToList();

			var hasMore = list.Count > limit;
			if (hasMore) list.RemoveAt(list.Count - 1);

			return (list, hasMore);
		}
	}
}
