using Domain.Entities;
using Infrastructure.InwContext;
using Infrastructure.Repositories.Interfaces;
using MongoDB.Driver;
using Shared.Exceptions;

namespace Infrastructure.Repositories.Implements
{
    public class PurchaserRepository : IPurchaserRepository
    {
        private readonly IMongoCollection<PurchaserEntity> _collection;

        public PurchaserRepository(MongoDBHelper mongoDBHelper)
        {
            mongoDBHelper.CreateCollectionIfNotExistsAsync("purchaser").Wait();
            _collection = mongoDBHelper.GetCollection<PurchaserEntity>("purchaser");
        }

        public async Task<PurchaserEntity> PurchasedFullAsync(PurchaserEntity entity)
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

        public async Task<bool> HasPurchasedFullAsync(string userId, string novelId)
        {
            try
            {
                var filter = Builders<PurchaserEntity>.Filter.Eq(p => p.user_id, userId) &
                    Builders<PurchaserEntity>.Filter.Eq(p => p.novel_id, novelId) &
                    Builders<PurchaserEntity>.Filter.Eq(p => p.is_full, true);

                var result = await _collection.Find(filter).AnyAsync();
                return result;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<int> CountPurchasedChaptersAsync(string userId, string novelId)
        {
            try
            {
                var filter = Builders<PurchaserEntity>.Filter.Eq(p => p.user_id, userId) &
                             Builders<PurchaserEntity>.Filter.Eq(p => p.novel_id, novelId) &
                             Builders<PurchaserEntity>.Filter.Eq(p => p.is_full, false);

                var purchaser = await _collection.Find(filter).FirstOrDefaultAsync();

                return purchaser?.chapter_id?.Count ?? 0;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<bool> HasPurchasedChapterAsync(string userId, string novelId, string chapterId)
        {
            try
            {
                var filter = Builders<PurchaserEntity>.Filter.And(
                    Builders<PurchaserEntity>.Filter.Eq(p => p.user_id, userId),
                    Builders<PurchaserEntity>.Filter.Eq(p => p.novel_id, novelId),
                    Builders<PurchaserEntity>.Filter.AnyEq(p => p.chapter_id, chapterId),
                    Builders<PurchaserEntity>.Filter.Eq(p => p.is_full, false)
                );

                var result = await _collection.Find(filter).AnyAsync();
                return result;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task AddChapterPurchaseAsync(string userId, string novelId, string chapterId)
        {
            try
            {
                var filter = Builders<PurchaserEntity>.Filter.And(
                    Builders<PurchaserEntity>.Filter.Eq(p => p.user_id, userId),
                    Builders<PurchaserEntity>.Filter.Eq(p => p.novel_id, novelId)
                );

                var update = Builders<PurchaserEntity>.Update.AddToSet(p => p.chapter_id, chapterId);

                var options = new UpdateOptions { IsUpsert = true };

                await _collection.UpdateOneAsync(filter, update, options);
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task TryMarkFullAsync(string userId, string novelId)
        {
            try
            {
                // var chapterFilter = Builders<PurchaserEntity>.Filter.And(
                //     Builders<PurchaserEntity>.Filter.Eq(c => c.user_id, userId),
                //     Builders<PurchaserEntity>.Filter.Eq(c => c.novel_id, novelId)
                // );

                // var purchasedChapters = await _collection.CountDocumentsAsync(chapterFilter);

                // var novel = await _novelCollection.Find(n => n.id == novelId).FirstOrDefaultAsync();
                // if (novel == null || purchasedChapters < novel.total_chapters) return;

                // var exists = await _purchaserCollection.Find(p =>
                //     p.user_id == userId &&
                //     p.novel_id == novelId &&
                //     p.is_full == true
                // ).FirstOrDefaultAsync();

                // if (exists != null) return;

                // PurchaserEntity fullPurchase = new()
                // {
                //     id = SystemHelper.RandomId(),
                //     user_id = userId,
                //     novel_id = novelId,
                //     is_full = true,
                //     created_at = DateTime.Now.Ticks
                // };

                // await _purchaserCollection.InsertOneAsync(fullPurchase);
            }
            catch
            {
                throw new InternalServerException();
            }
        }
    }
}