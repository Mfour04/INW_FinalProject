using Domain.Entities;
using Infrastructure.InwContext;
using Infrastructure.Repositories.Interfaces;
using MongoDB.Driver;
using Shared.Exceptions;

namespace Infrastructure.Repositories.Implements
{
    public class BadgeProgressRepository : IBadgeProgressRepository
    {
        private readonly IMongoCollection<BadgeProgressEntity> _collection;

        public BadgeProgressRepository(MongoDBHelper mongoDBHelper)
        {
            mongoDBHelper.CreateCollectionIfNotExistsAsync("badge_progress").Wait();
            _collection = mongoDBHelper.GetCollection<BadgeProgressEntity>("badge_progress");
        }

        public async Task<bool> ExistsAsync(string userId, string badgeId)
        {
            return await _collection.Find(x => x.user_id == userId && x.badge_id == badgeId).AnyAsync();
        }

        public async Task CreateManyAsync(List<BadgeProgressEntity> progresses)
        {
            if (progresses.Count > 0)
                await _collection.InsertManyAsync(progresses);
        }

          public async Task<List<BadgeProgressEntity>> GetByUserIdAsync(string userId)
        {
            try
            {
                var filter = Builders<BadgeProgressEntity>.Filter.Eq(x => x.user_id, userId);
                return await _collection.Find(filter).ToListAsync();
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<List<BadgeProgressEntity>> GetCompletedByUserIdAsync(string userId)
        {
            try
            {
                var filter = Builders<BadgeProgressEntity>.Filter.And(
                    Builders<BadgeProgressEntity>.Filter.Eq(x => x.user_id, userId),
                    Builders<BadgeProgressEntity>.Filter.Eq(x => x.is_completed, true)
                );
                return await _collection.Find(filter).ToListAsync();
            }
            catch
            {
                throw new InternalServerException();
            }
        }
    }
}