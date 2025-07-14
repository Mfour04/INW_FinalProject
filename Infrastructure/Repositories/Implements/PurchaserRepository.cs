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

        /// <summary>
        /// Tạo mới người dùng đã mua chương hoặc truyện
        /// </summary>
        public async Task<PurchaserEntity> CreateAsync(PurchaserEntity entity)
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
        /// Kiểm tra người dùng đã mua toàn bộ truyện hay chưa.
        /// </summary>
        public async Task<bool> HasPurchasedFullAsync(string userId, string novelId)
        {
            try
            {
                var filter = Builders<PurchaserEntity>.Filter.Eq(p => p.user_id, userId) &
                             Builders<PurchaserEntity>.Filter.Eq(p => p.novel_id, novelId) &
                             Builders<PurchaserEntity>.Filter.Eq(p => p.is_full, true);

                return await _collection.Find(filter).AnyAsync();
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        /// <summary>
        /// Kiểm tra người dùng đã mua một chương cụ thể chưa.
        /// </summary>
        public async Task<bool> HasPurchasedChapterAsync(string userId, string novelId, string chapterId)
        {
            try
            {
                var filter = Builders<PurchaserEntity>.Filter.And(
                    Builders<PurchaserEntity>.Filter.Eq(p => p.user_id, userId),
                    Builders<PurchaserEntity>.Filter.Eq(p => p.novel_id, novelId),
                    Builders<PurchaserEntity>.Filter.AnyEq(p => p.chapter_ids, chapterId),
                    Builders<PurchaserEntity>.Filter.Eq(p => p.is_full, false)
                );

                return await _collection.Find(filter).AnyAsync();
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        /// <summary>
        /// Lấy danh sách chương đã mua của người dùng trong truyện
        /// </summary>
        public async Task<List<string>> GetPurchasedChaptersAsync(string userId, string novelId)
        {
            try
            {
                var filter = Builders<PurchaserEntity>.Filter.And(
                    Builders<PurchaserEntity>.Filter.Eq(x => x.user_id, userId),
                    Builders<PurchaserEntity>.Filter.Eq(x => x.novel_id, novelId),
                    Builders<PurchaserEntity>.Filter.Eq(x => x.is_full, false)
                );

                var purchaser = await _collection.Find(filter).FirstOrDefaultAsync();
                return purchaser?.chapter_ids ?? new List<string>();
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        /// <summary>
        /// Thêm một chương đã mua vào danh sách chapter_ids
        /// </summary>
        public async Task<bool> AddChapterAsync(string userId, string novelId, string chapterId)
        {
            try
            {
                var filter = Builders<PurchaserEntity>.Filter.And(
                    Builders<PurchaserEntity>.Filter.Eq(x => x.user_id, userId),
                    Builders<PurchaserEntity>.Filter.Eq(x => x.novel_id, novelId),
                    Builders<PurchaserEntity>.Filter.Eq(x => x.is_full, false)
                );

                var update = Builders<PurchaserEntity>.Update
                    .AddToSet(x => x.chapter_ids, chapterId)
                    .Inc(x => x.chap_snapshot, 1)
                    .Set(x => x.updated_at, DateTime.Now.Ticks);

                var result = await _collection.UpdateOneAsync(filter, update);
                return result.ModifiedCount > 0;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        /// <summary>
        /// Lấy thông tin mua truyện theo user + novel
        /// </summary>
        public async Task<PurchaserEntity> GetByUserAndNovelAsync(string userId, string novelId)
        {
            try
            {
                var filter = Builders<PurchaserEntity>.Filter.And(
                    Builders<PurchaserEntity>.Filter.Eq(p => p.user_id, userId),
                    Builders<PurchaserEntity>.Filter.Eq(p => p.novel_id, novelId)
                );

                return await _collection.Find(filter).FirstOrDefaultAsync();
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        /// <summary>
        /// Cập nhật bản ghi Purchaser theo id
        /// </summary>
        public async Task<bool> UpdateAsync(string id, PurchaserEntity entity)
        {
            try
            {
                var filter = Builders<PurchaserEntity>.Filter.Eq(p => p.id, id);

                var purchaser = await _collection.Find(filter).FirstOrDefaultAsync();

                var update = Builders<PurchaserEntity>.Update
                    .Set(p => p.is_full, entity.is_full)
                    .Set(p => p.chap_snapshot, entity.chap_snapshot)
                    .Set(p => p.chapter_ids, entity.chapter_ids)
                    .Set(p => p.updated_at, DateTime.Now.Ticks);

                var updated = await _collection.FindOneAndUpdateAsync(
                    filter,
                    update,
                    new FindOneAndUpdateOptions<PurchaserEntity>
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
    }
}
