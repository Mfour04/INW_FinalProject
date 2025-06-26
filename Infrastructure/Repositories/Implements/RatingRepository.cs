using DnsClient;
using Domain.Entities;
using Infrastructure.InwContext;
using Infrastructure.Repositories.Interfaces;
using MongoDB.Driver;
using Shared.Exceptions;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.Implements
{
    public class RatingRepository : IRatingRepository
    {
        private readonly IMongoCollection<RatingEntity> _ratings;
        private readonly IMongoCollection<NovelEntity> _novels;
        public RatingRepository(MongoDBHelper mongoDBHelper)
        {
            mongoDBHelper.CreateCollectionIfNotExistsAsync("ratings").Wait();
            _ratings = mongoDBHelper.GetCollection<RatingEntity>("ratings");
            _novels = mongoDBHelper.GetCollection<NovelEntity>("novel");
        }
        public async Task<RatingEntity> CreateAsync(RatingEntity rating)
        {
            try
            {
                rating.created_at = DateTime.UtcNow.Ticks;
                rating.updated_at = DateTime.UtcNow.Ticks;

                await _ratings.InsertOneAsync(rating);
                await UpdateNovelRatingStats(rating.novel_id);

                return rating;
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
                var rating = await GetByIdAsync(id);
                if (rating == null) return false;

                var filter = Builders<RatingEntity>.Filter.Eq(r => r.id, id);
                var result = await _ratings.DeleteOneAsync(filter);

                if (result.DeletedCount > 0)
                {
                    await UpdateNovelRatingStats(rating.novel_id);
                    return true;
                }

                return false;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<List<RatingEntity>> GetAllAsync()
        {
            try
            {
                return await _ratings.Find(_ => true).ToListAsync();
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<double> GetAverageRatingByNovelIdAsync(string novelId)
        {
            try
            {
                var filter = Builders<RatingEntity>.Filter.Eq(r => r.novel_id, novelId);
                var ratings = await _ratings.Find(filter).ToListAsync();

                if (!ratings.Any()) return 0;

                return Math.Round(ratings.Average(r => r.score), 2);
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<RatingEntity> GetByIdAsync(string id)
        {
            try
            {
                var filter = Builders<RatingEntity>.Filter.Eq(r => r.id, id);
                return await _ratings.Find(filter).FirstOrDefaultAsync();
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<List<RatingEntity>> GetByNovelIdAsync(string novelId)
        {
            try
            {
                var filter = Builders<RatingEntity>.Filter.Eq(r => r.novel_id, novelId);
                return await _ratings.Find(filter).ToListAsync();

            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<RatingEntity> GetByUserAndNovelAsync(string userId, string novelId)
        {
            try
            {
                var filter = Builders<RatingEntity>.Filter.And(
                    Builders<RatingEntity>.Filter.Eq(r => r.user_id, userId),
                    Builders<RatingEntity>.Filter.Eq(r => r.novel_id, novelId)
                );
                return await _ratings.Find(filter).FirstOrDefaultAsync();
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<int> GetRatingCountByNovelIdAsync(string novelId)
        {
            try
            {
                var filter = Builders<RatingEntity>.Filter.Eq(r => r.novel_id, novelId);
                return (int)await _ratings.CountDocumentsAsync(filter);
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<RatingEntity> UpdateAsync(RatingEntity rating)
        {
            try
            {
                rating.updated_at = DateTime.UtcNow.Ticks;

                var filter = Builders<RatingEntity>.Filter.Eq(r => r.id, rating.id);
                await _ratings.ReplaceOneAsync(filter, rating);
                await UpdateNovelRatingStats(rating.novel_id);

                return rating;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        private async Task UpdateNovelRatingStats(string novelId)
        {
            try
            {
                Console.WriteLine($"Updating stats for novel: {novelId}");

                var avgRating = await GetAverageRatingByNovelIdAsync(novelId);
                var ratingCount = await GetRatingCountByNovelIdAsync(novelId);

                Console.WriteLine($"Calculated - Avg: {avgRating}, Count: {ratingCount}");

                var filter = Builders<NovelEntity>.Filter.Eq(n => n.id, novelId);
                var update = Builders<NovelEntity>.Update
                    .Set(n => n.rating_avg, avgRating)
                    .Set(n => n.rating_count, ratingCount)
                    .Set(n => n.updated_at, DateTime.UtcNow.Ticks); 

                var result = await _novels.UpdateOneAsync(filter, update);

                Console.WriteLine($"Novel update result - ModifiedCount: {result.ModifiedCount}");

                if (result.ModifiedCount == 0)
                {
                    Console.WriteLine($"Warning: No novel updated for id: {novelId}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdateNovelRatingStats: {ex.Message}");
                throw;
            }
        }
    }
}
