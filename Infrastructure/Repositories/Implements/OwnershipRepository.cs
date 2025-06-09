using Domain.Entities;
using Domain.Entities.System;
using Infrastructure.InwContext;
using Infrastructure.Repositories.Interfaces;
using MongoDB.Driver;
using Shared.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.Implements
{
    public class OwnershipRepository : IOwnershipRepository
    {
        private readonly IMongoCollection<OwnershipEntity> _collection;
        public OwnershipRepository(MongoDBHelper mongoDBHelper)
        {
            mongoDBHelper.CreateCollectionIfNotExistsAsync("owner").Wait();
            _collection = mongoDBHelper.GetCollection<OwnershipEntity>("owner");
        }
        public async Task<OwnershipEntity> CreateOwnerShipAsync(OwnershipEntity entity)
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

        public async Task<bool> DeleteOwnerShipAsync(string id)
        {
            try
            {
                var filter = Builders<OwnershipEntity>.Filter.Eq(x => x.id, id);
                var result = await _collection.FindOneAndDeleteAsync(filter);

                return result != null;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<List<OwnershipEntity>> GetAllOwnerShipAsync(FindCreterias creterias)
        {
            try
            {
                var builder = Builders<OwnershipEntity>.Filter;
                var filtered = builder.Empty;

                var query = _collection
                    .Find(filtered)
                    .Skip(creterias.Page * creterias.Limit)
                    .Limit(creterias.Limit);

                return await query.ToListAsync();
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<OwnershipEntity> GetByOwnerShipIdAsync(string ownershipId)
        {
            try
            {
                var result = await _collection.Find(x => x.id == ownershipId).FirstOrDefaultAsync();
                return result;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<List<string>> GetOwnedChapterIdsAsync(string userId, string novelId)
        {
            try
            {
                var filter = Builders<OwnershipEntity>.Filter.And(
                    Builders<OwnershipEntity>.Filter.Eq(x => x.user_id, userId),
                    Builders<OwnershipEntity>.Filter.Eq(x => x.novel_id, novelId)
                );

                var ownerShip = await _collection.Find(filter).FirstOrDefaultAsync();

                return ownerShip?.chapter_id ?? new List<string>();
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<bool> HasAnyChapterOwnershipAsync(string userId, string novelId)
        {
            try
            {
                var filter = Builders<OwnershipEntity>.Filter.And(
                    Builders<OwnershipEntity>.Filter.Eq(x => x.user_id, userId),
                    Builders<OwnershipEntity>.Filter.Eq(x => x.novel_id, novelId),
                    Builders<OwnershipEntity>.Filter.SizeGt(x => x.chapter_id, 0)
                );

                return await _collection.Find(filter).AnyAsync();
            } catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<bool> HasChapterOwnershipAsync(string userId, string novelId, string chapterId)
        {
            try
            {
                var filter = Builders<OwnershipEntity>.Filter.And(
                    Builders<OwnershipEntity>.Filter.Eq(x => x.user_id, userId),
                    Builders<OwnershipEntity>.Filter.Eq(x => x.novel_id, novelId),
                    Builders<OwnershipEntity>.Filter.SizeGt(x => x.chapter_id, 0)
                );
                return await _collection.Find(filter).AnyAsync();
            }
            catch
            {
                throw new InternalServerException();
            }
        }

       
        public async Task<bool> HasFullNovelOwnershipAsync(string userId, string novelId)
        {
            try
            {
                var filter = Builders<OwnershipEntity>.Filter.And(
                    Builders<OwnershipEntity>.Filter.Eq(x => x.user_id, userId),
                    Builders<OwnershipEntity>.Filter.Eq(x => x.novel_id, novelId),
                    Builders<OwnershipEntity>.Filter.Eq(x => x.is_full, true)
                );

                return await _collection.Find(filter).AnyAsync();
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<OwnershipEntity> UpdateOwnerShipAsync(OwnershipEntity entity)
        {
            try
            {
                var filter = Builders<OwnershipEntity>.Filter.Eq(x => x.id, entity.id);
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
