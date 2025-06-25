using Domain.Entities;
using Infrastructure.InwContext;
using Infrastructure.Repositories.Interfaces;
using MongoDB.Driver;
using Shared.Exceptions;
using Shared.Helpers;

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
        /// Đếm số chương người dùng đã mua theo từng chương.
        /// </summary>
        public async Task<int> CountPurchasedChaptersAsync(string userId, string novelId)
        {
            try
            {
                var filter = Builders<PurchaserEntity>.Filter.Eq(p => p.user_id, userId) &
                             Builders<PurchaserEntity>.Filter.Eq(p => p.novel_id, novelId) &
                             Builders<PurchaserEntity>.Filter.Eq(p => p.is_full, false);

                var purchaser = await _collection.Find(filter).FirstOrDefaultAsync();
                return purchaser?.chapter_ids?.Count ?? 0;
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
        /// Thêm chương đã mua vào danh sách chương của người dùng.
        /// </summary>
        public async Task AddChapterPurchaseAsync(string userId, string novelId, string chapterId)
        {
            try
            {
                var filter = Builders<PurchaserEntity>.Filter.And(
                    Builders<PurchaserEntity>.Filter.Eq(p => p.user_id, userId),
                    Builders<PurchaserEntity>.Filter.Eq(p => p.novel_id, novelId),
                    Builders<PurchaserEntity>.Filter.Eq(p => p.is_full, false)
                );

                var existing = await _collection.Find(filter).FirstOrDefaultAsync();

                if (existing == null)
                {
                    PurchaserEntity newPurchase = new()
                    {
                        id = SystemHelper.RandomId(),
                        user_id = userId,
                        novel_id = novelId,
                        is_full = false,
                        chapter_ids = new List<string> { chapterId },
                        created_at = DateTime.Now.Ticks
                    };

                    await _collection.InsertOneAsync(newPurchase);
                }
                else
                {
                    var update = Builders<PurchaserEntity>.Update
                        .AddToSet(p => p.chapter_ids, chapterId)
                        .Set(p => p.updated_at, DateTime.Now.Ticks);

                    await _collection.UpdateOneAsync(filter, update);
                }
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        /// <summary>
        /// Thêm thông tin mua full truyện.
        /// </summary>
        public async Task AddFullNovelPurchaseAsync(PurchaserEntity entity)
        {
            try
            {
                // Tìm bản ghi hiện tại đã mua lẻ chương (is_full = false)
                var filter = Builders<PurchaserEntity>.Filter.Eq(p => p.user_id, entity.user_id) &
                             Builders<PurchaserEntity>.Filter.Eq(p => p.novel_id, entity.novel_id) &
                             Builders<PurchaserEntity>.Filter.Eq(p => p.is_full, false);

                var existing = await _collection.Find(filter).FirstOrDefaultAsync();

                if (existing != null)
                {
                    // Cập nhật trạng thái lên mua toàn bộ truyện
                    var update = Builders<PurchaserEntity>.Update
                        .Set(p => p.is_full, true)
                        .Set(p => p.chapter_ids, entity.chapter_ids)
                        .Set(p => p.full_chap_count, entity.full_chap_count)
                        .Set(p => p.updated_at, DateTime.Now.Ticks);

                    await _collection.UpdateOneAsync(filter, update);
                }
                else
                {
                    // Nếu chưa có bản ghi nào → insert mới
                    await _collection.InsertOneAsync(entity);
                }
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        /// <summary>
        /// Cập nhật trạng thái mua của người dùng (is_full, full_chap_count).
        /// </summary>
        public async Task<bool> UpdatePurchaseStatusAsync(string id, bool isFull, int fullChapterCount)
        {
            try
            {
                var filter = Builders<PurchaserEntity>.Filter.Eq(x => x.id, id);

                var update = Builders<PurchaserEntity>.Update
                    .Set(x => x.is_full, isFull)
                    .Set(x => x.full_chap_count, fullChapterCount)
                    .Set(x => x.updated_at, DateTime.Now.Ticks);

                var updated = await _collection.FindOneAndUpdateAsync(
                    filter,
                    update,
                    new FindOneAndUpdateOptions<PurchaserEntity> { ReturnDocument = ReturnDocument.After }
                );

                return updated != null;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        /// <summary>
        /// Thử đánh dấu là đã mua full nếu đủ số chương.
        /// </summary>
        public async Task TryMarkFullAsync(string userId, string novelId, int totalChapters)
        {
            try
            {
                var filter = Builders<PurchaserEntity>.Filter.Eq(p => p.user_id, userId) &
                             Builders<PurchaserEntity>.Filter.Eq(p => p.novel_id, novelId) &
                             Builders<PurchaserEntity>.Filter.Eq(p => p.is_full, false);

                var purchaser = await _collection.Find(filter).FirstOrDefaultAsync();
                if (purchaser == null || (purchaser.chapter_ids?.Count ?? 0) < totalChapters)
                    return;

                await UpdatePurchaseStatusAsync(purchaser.id, true, totalChapters);
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        /// <summary>
        /// Nếu truyện đã thêm chương mới, cập nhật trạng thái mua full thành false.
        /// </summary>
        public async Task ValidateFullPurchaseAsync(string userId, string novelId, int currentTotalChapters)
        {
            try
            {
                var filter = Builders<PurchaserEntity>.Filter.Eq(p => p.user_id, userId) &
                             Builders<PurchaserEntity>.Filter.Eq(p => p.novel_id, novelId) &
                             Builders<PurchaserEntity>.Filter.Eq(p => p.is_full, true);

                var purchaser = await _collection.Find(filter).FirstOrDefaultAsync();
                if (purchaser == null) return;

                if (purchaser.full_chap_count < currentTotalChapters)
                {
                    await UpdatePurchaseStatusAsync(purchaser.id, false, purchaser.full_chap_count);
                }
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        /// <summary>
        /// Lấy danh sách ID các chương mà người dùng đã mua lẻ.
        /// </summary>
        public async Task<List<string>> GetPurchasedChapterIdsAsync(string userId, string novelId)
        {
            try
            {
                var filter = Builders<PurchaserEntity>.Filter.Eq(p => p.user_id, userId) &
                             Builders<PurchaserEntity>.Filter.Eq(p => p.novel_id, novelId) &
                             Builders<PurchaserEntity>.Filter.Eq(p => p.is_full, false);

                var purchaser = await _collection.Find(filter).FirstOrDefaultAsync();
                return purchaser?.chapter_ids ?? new List<string>();
            }
            catch
            {
                throw new InternalServerException();
            }
        }
    }
}
