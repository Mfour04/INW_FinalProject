using Domain.Entities;
using Domain.Entities.System;
using Infrastructure.InwContext;
using Infrastructure.Repositories.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;
using Shared.Exceptions;

namespace Infrastructure.Repositories.Implements
{
    public class NovelFollowRepository : INovelFollowRepository
    {
        private readonly IMongoCollection<NovelFollowerEntity> _collection;
        public NovelFollowRepository(MongoDBHelper mongoDBHelper)
        {
            mongoDBHelper.CreateCollectionIfNotExistsAsync("novel_follower").Wait();
            _collection = mongoDBHelper.GetCollection<NovelFollowerEntity>("novel_follower");
        }
        public async Task<NovelFollowerEntity> CreateNovelFollowAsync(NovelFollowerEntity entity)
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

        public async Task<bool> DeleteNovelFollowAsync(string id)
        {
            try
            {
                var filter = Builders<NovelFollowerEntity>.Filter.Eq(x => x.id, id);
                var deleted = await _collection.FindOneAndDeleteAsync(filter);

                return deleted != null;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<List<NovelFollowerEntity>> GetAllNovelFollowAsync()
        {
            try
            {
                return await _collection.Find(_ => true).ToListAsync();
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<NovelFollowerEntity> GetByNovelFollowIdAsync(string novelfollowId)
        {
            try
            {
                var result = await _collection.Find(x => x.id == novelfollowId).FirstOrDefaultAsync();
                return result;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<(List<NovelFollowerEntity> NovelFollows, int TotalCount)> GetFollowedNovelsByUserIdAsync(string userId, FindCreterias findCreterias)
        {
            try
            {
                var filter = Builders<NovelFollowerEntity>.Filter.Eq(x => x.user_id, userId);

                var totalCount = await _collection.CountDocumentsAsync(filter);

                var follows = await _collection.Find(filter)
                    .Skip(findCreterias.Page * findCreterias.Limit)
                    .Limit(findCreterias.Limit)
                    .ToListAsync();

                return (follows, (int)totalCount);
            }
            catch (Exception ex)
            {
                throw new InternalServerException("Failed to retrieve followed novels");
            }
        }


        public async Task<List<NovelFollowerEntity>> GetFollowersByNovelIdAsync(string novelId)
        {
            try
            {
                return await _collection.Find(x => x.novel_id == novelId).ToListAsync();
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<NovelFollowerEntity?> GetByUserAndNovelIdAsync(string userId, string novelId)
        {
            try
            {
                return await _collection
                .Find(x => x.user_id == userId && x.novel_id == novelId)
                .FirstOrDefaultAsync();
            }
            catch
            {
                throw new InternalServerException();
            }
            
        }

        public async Task<NovelFollowerEntity> UpdateNovelFollowAsync(NovelFollowerEntity entity)
        {
            try
            {
                var filter = Builders<NovelFollowerEntity>.Filter.Eq(x => x.id, entity.id);
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
