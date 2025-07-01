using Domain.Entities;
using Infrastructure.InwContext;
using Infrastructure.Repositories.Interfaces;
using MongoDB.Driver;
using Shared.Exceptions;

namespace Infrastructure.Repositories.Implements
{
    public class NovelViewTrackingRepository : INovelViewTrackingRepository
    {
        private readonly IMongoCollection<NovelViewTrackingEntity> _collection;

        public NovelViewTrackingRepository(MongoDBHelper mongoDBHelper)
        {
            _collection = mongoDBHelper.GetCollection<NovelViewTrackingEntity>("novel_view_tracking");
        }

        public async Task<NovelViewTrackingEntity> CreateViewTrackingAsync(NovelViewTrackingEntity entity)
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

        public async Task<NovelViewTrackingEntity?> FindByUserAndNovelAsync(string userId, string novelId)
        {
            try
            {
                var filtered = Builders<NovelViewTrackingEntity>.Filter.And(
                    Builders<NovelViewTrackingEntity>.Filter.Eq(x => x.user_id, userId),
                    Builders<NovelViewTrackingEntity>.Filter.Eq(x => x.novel_id, novelId)
                    );

                return await _collection.Find(filtered).FirstOrDefaultAsync();
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<NovelViewTrackingEntity> UpdateViewTrackingAsync(NovelViewTrackingEntity entity)
        {
            try
            {
                var filtered = Builders<NovelViewTrackingEntity>.Filter.Eq(x => x.id, entity.id);
                var update = Builders<NovelViewTrackingEntity>.Update.Set(x => x.updated_at, entity.updated_at);
                await _collection.UpdateOneAsync(filtered, update);

                return entity;
            }
            catch
            {
                throw new InternalServerException();
            }
        }
    }
}
