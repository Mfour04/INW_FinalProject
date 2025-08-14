using Domain.Entities;
using Domain.Entities.System;
using Infrastructure.InwContext;
using Infrastructure.Repositories.Interfaces;
using MongoDB.Driver;
using Shared.Exceptions;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.Implements
{
    public class ReadingProcessRepository : IReadingProcessRepository
    {
        public readonly IMongoCollection<ReadingProcessEntity> _collection;
        public ReadingProcessRepository(MongoDBHelper mongoDBHelper)
        {
            mongoDBHelper.CreateCollectionIfNotExistsAsync("reading_processes").Wait();
            _collection = mongoDBHelper.GetCollection<ReadingProcessEntity>("reading_processes");
        }
        public async Task<ReadingProcessEntity> CreateAsync(ReadingProcessEntity entity)
        {
            try
            {
                entity.created_at = TimeHelper.NowTicks;
                entity.updated_at = TimeHelper.NowTicks;
                await _collection.InsertOneAsync(entity);
                return entity;
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
                var filter = Builders<ReadingProcessEntity>.Filter.Eq(e => e.id, id);
                var result = await _collection.DeleteOneAsync(filter);
                return result.DeletedCount > 0;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<bool> ExistsAsync(string userId, string novelId)
        {
            try
            {
                var filter = Builders<ReadingProcessEntity>.Filter.And(
                    Builders<ReadingProcessEntity>.Filter.Eq(e => e.user_id, userId),
                    Builders<ReadingProcessEntity>.Filter.Eq(e => e.novel_id, novelId)
                );
                var result = await _collection.CountDocumentsAsync(filter);
                return result > 0;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<ReadingProcessEntity> GetByIdAsync(string id)
        {
            try
            {
                var filter = Builders<ReadingProcessEntity>.Filter.Eq(e => e.id, id);
                var result = await _collection.Find(filter).FirstOrDefaultAsync();
                return result;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<ReadingProcessEntity> GetByUserAndNovelAsync(string userId, string novelId)
        {
            try
            {
                var filter = Builders<ReadingProcessEntity>.Filter.And(
                    Builders<ReadingProcessEntity>.Filter.Eq(e => e.user_id, userId),
                    Builders<ReadingProcessEntity>.Filter.Eq(e => e.novel_id, novelId)
                );
                var result = await _collection.Find(filter).FirstOrDefaultAsync();
                return result;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<List<ReadingProcessEntity>> GetReadingHistoryAsync(FindCreterias findCreterias, string userId)
        {
            try
            {
                var filter = Builders<ReadingProcessEntity>.Filter.Eq(e => e.user_id, userId);
                var result = await _collection.Find(filter)
                    .SortByDescending(x => x.updated_at)
                    .Skip(findCreterias.Page * findCreterias.Limit)
                    .Limit(findCreterias.Limit)
                    .ToListAsync();
                return result;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<ReadingProcessEntity> UpdateAsync(ReadingProcessEntity entity)
        {
            try
            {
                entity.updated_at = TimeHelper.NowTicks;
                var filter = Builders<ReadingProcessEntity>.Filter.Eq(e => e.id, entity.id);
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
