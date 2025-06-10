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
        private readonly IMongoCollection<PurchaserEntity> _collection;
        public OwnershipRepository(MongoDBHelper mongoDBHelper)
        {
            mongoDBHelper.CreateCollectionIfNotExistsAsync("owner").Wait();
            _collection = mongoDBHelper.GetCollection<PurchaserEntity>("owner");
        }
        public async Task<PurchaserEntity> CreateOwnerShipAsync(PurchaserEntity entity)
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
                var filter = Builders<PurchaserEntity>.Filter.Eq(x => x.id, id);
                var result = await _collection.FindOneAndDeleteAsync(filter);

                return result != null;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<List<PurchaserEntity>> GetAllOwnerShipAsync(FindCreterias creterias)
        {
            try
            {
                var builder = Builders<PurchaserEntity>.Filter;
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

        public async Task<PurchaserEntity> GetByOwnerShipIdAsync(string ownershipId)
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
                var filter = Builders<PurchaserEntity>.Filter.And(
                    Builders<PurchaserEntity>.Filter.Eq(x => x.user_id, userId),
                    Builders<PurchaserEntity>.Filter.Eq(x => x.novel_id, novelId)
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
                var filter = Builders<PurchaserEntity>.Filter.And(
                    Builders<PurchaserEntity>.Filter.Eq(x => x.user_id, userId),
                    Builders<PurchaserEntity>.Filter.Eq(x => x.novel_id, novelId),
                    Builders<PurchaserEntity>.Filter.SizeGt(x => x.chapter_id, 0)
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
                var filter = Builders<PurchaserEntity>.Filter.And(
                    Builders<PurchaserEntity>.Filter.Eq(x => x.user_id, userId),
                    Builders<PurchaserEntity>.Filter.Eq(x => x.novel_id, novelId),
                    Builders<PurchaserEntity>.Filter.SizeGt(x => x.chapter_id, 0)
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
                var filter = Builders<PurchaserEntity>.Filter.And(
                    Builders<PurchaserEntity>.Filter.Eq(x => x.user_id, userId),
                    Builders<PurchaserEntity>.Filter.Eq(x => x.novel_id, novelId),
                    Builders<PurchaserEntity>.Filter.Eq(x => x.is_full, true)
                );

                return await _collection.Find(filter).AnyAsync();
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<PurchaserEntity> UpdateOwnerShipAsync(PurchaserEntity entity)
        {
            try
            {
                var filter = Builders<PurchaserEntity>.Filter.Eq(x => x.id, entity.id);
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
