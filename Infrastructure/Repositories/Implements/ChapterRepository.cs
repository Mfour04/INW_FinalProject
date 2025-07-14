using Domain.Entities;
using Domain.Entities.System;
using Infrastructure.InwContext;
using Infrastructure.Repositories.Interfaces;
using MongoDB.Driver;
using Shared.Exceptions;

namespace Infrastructure.Repositories.Implements
{
    public class ChapterRepository : IChapterRepository
    {
        private readonly IMongoCollection<ChapterEntity> _collection;
        public ChapterRepository(MongoDBHelper mongoDBHelper)
        {
            mongoDBHelper.CreateCollectionIfNotExistsAsync("chapter").Wait();
            _collection = mongoDBHelper.GetCollection<ChapterEntity>("chapter");
        }

        /// <summary>
        /// Tạo mới một chapter
        /// </summary>
        public async Task<ChapterEntity> CreateAsync(ChapterEntity entity)
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
        /// Cập nhật thông tin chapter
        /// </summary>
        public async Task<ChapterEntity> UpdateAsync(ChapterEntity entity)
        {
            try
            {
                entity.updated_at = DateTime.UtcNow.Ticks;
                var filter = Builders<ChapterEntity>.Filter.Eq(x => x.id, entity.id);
                await _collection.ReplaceOneAsync(filter, entity);
                return entity;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        /// <summary>
        /// Xóa chapter
        /// </summary>
        public async Task<bool> DeleteAsync(string id)
        {
            try
            {
                var filter = Builders<ChapterEntity>.Filter.Eq(x => x.id, id);
                var deleted = await _collection.FindOneAndDeleteAsync(filter);
                return deleted != null;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        /// <summary>
        /// Lấy chapter theo ID
        /// <//summary>
        public async Task<ChapterEntity> GetByIdAsync(string chapterId)
        {
            try
            {
                return await _collection.Find(x => x.id == chapterId).FirstOrDefaultAsync();
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        /// <summary>
        /// Lấy danh sách chapter theo danh sách ID
        /// /summary>
        public async Task<List<ChapterEntity>> GetChaptersByIdsAsync(List<string> chapterIds)
        {
            try
            {
                var filter = Builders<ChapterEntity>.Filter.In(x => x.id, chapterIds);
                return await _collection.Find(filter).ToListAsync();
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        /// <summary>
        /// Lấy tất cả chapter có phân trang
        /// </summary>
        public async Task<List<ChapterEntity>> GetAllAsync(FindCreterias creterias)
        {
            try
            {
                var query = _collection.Find(Builders<ChapterEntity>.Filter.Empty)
                                       .Skip(creterias.Page * creterias.Limit)
                                       .Limit(creterias.Limit);
                return await query.ToListAsync();
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        /// <summary>
        /// Lấy tất cả chapter theo novel ID
        /// </summary>
        public async Task<List<ChapterEntity>> GetAllByNovelIdAsync(string novelId)
        {
            try
            {
                var filter = Builders<ChapterEntity>.Filter.Eq(x => x.novel_id, novelId);
                return await _collection.Find(filter).SortBy(x => x.chapter_number).ToListAsync();
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        /// <summary>
        /// Lấy danh sách chapter phân trang, lọc theo điều kiện
        /// </summary>
        public async Task<(List<ChapterEntity> Chapters, int TotalChapters, int TotalPages)> GetPagedByNovelIdAsync(string novelId, ChapterFindCreterias creterias, List<SortCreterias> sortCreterias)
        {
            try
            {
                var builder = Builders<ChapterEntity>.Filter;
                var filtered = builder.Eq(c => c.novel_id, novelId);
                if (creterias.ChapterNumber.HasValue)
                    filtered &= builder.Eq(c => c.chapter_number, creterias.ChapterNumber.Value);

                var sortBuilder = Builders<ChapterEntity>.Sort;
                var sortDefinitions = sortCreterias.Select(criterion =>
                    criterion.Field switch
                    {
                        "chapter_number" => criterion.IsDescending
                            ? sortBuilder.Descending(x => x.chapter_number)
                            : sortBuilder.Ascending(x => x.chapter_number),
                        _ => null
                    }).Where(sd => sd != null).ToList();

                var query = _collection.Find(filtered);
                if (sortDefinitions.Any())
                    query = query.Sort(sortBuilder.Combine(sortDefinitions));

                var totalChapters = (int)await _collection.CountDocumentsAsync(filtered);
                var totalPages = (int)Math.Ceiling((double)totalChapters / creterias.Limit);
                var result = await query.Skip(creterias.Page * creterias.Limit).Limit(creterias.Limit).ToListAsync();

                return (result, totalChapters, totalPages);
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        /// <summary>
        /// Lấy danh sách ID chapter của truyện
        /// </summary>
        public async Task<List<string>> GetIdsByNovelIdAsync(string novelId)
        {
            try
            {
                return await _collection.Find(c => c.novel_id == novelId).Project(c => c.id).ToListAsync();
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        /// <summary>
        /// Lấy danh sách ID chương miễn phí
        /// </summary>
        public async Task<List<string>> GetFreeIdsByNovelIdAsync(string novelId)
        {
            try
            {
                var filter = Builders<ChapterEntity>.Filter.And(
                    Builders<ChapterEntity>.Filter.Eq(c => c.novel_id, novelId),
                    Builders<ChapterEntity>.Filter.Eq(c => c.is_paid, false));
                return await _collection.Find(filter).Project(c => c.id).ToListAsync();
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        /// <summary>
        /// Lấy danh sách chapter sắp xếp theo số chương
        /// </summary>
        public async Task<List<ChapterEntity>> GetSequentialByNovelIdAsync(string novelId)
        {
            try
            {
                var filter = Builders<ChapterEntity>.Filter.Eq(x => x.novel_id, novelId);
                return await _collection.Find(filter).SortBy(x => x.chapter_number).ToListAsync();
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        /// <summary>
        /// Lấy danh sách chapter miễn phí
        /// </summary>
        public async Task<List<ChapterEntity>> GetFreeByNovelIdAsync(string novelId)
        {
            try
            {
                var filter = Builders<ChapterEntity>.Filter.And(
                    Builders<ChapterEntity>.Filter.Eq(x => x.novel_id, novelId),
                    Builders<ChapterEntity>.Filter.Eq(x => x.is_paid, false)
                );

                return await _collection.Find(filter).ToListAsync();
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        /// <summary>
        /// Lấy danh sách chapter đã public
        /// </summary>
        public async Task<List<ChapterEntity>> GetPublishedByNovelIdAsync(string novelId)
        {
            try
            {
                var filter = Builders<ChapterEntity>.Filter.Eq(c => c.novel_id, novelId) &
                             Builders<ChapterEntity>.Filter.Eq(c => c.is_draft, false) &
                             Builders<ChapterEntity>.Filter.Eq(c => c.is_public, true);
                return await _collection.Find(filter).SortBy(c => c.chapter_number).ToListAsync();
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        /// <summary>
        /// Lấy chapter cuối cùng đã public
        /// </summary>
        public async Task<ChapterEntity?> GetLastPublishedAsync(string novelId)
        {
            try
            {
                var filter = Builders<ChapterEntity>.Filter.Eq(c => c.novel_id, novelId) &
                             Builders<ChapterEntity>.Filter.Eq(c => c.is_draft, false) &
                             Builders<ChapterEntity>.Filter.Eq(c => c.is_public, true);
                return await _collection.Find(filter).SortByDescending(c => c.chapter_number).FirstOrDefaultAsync();
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        /// <summary>
        /// Lấy chương trước đó
        /// </summary>
        public async Task<ChapterEntity?> GetPreviousAsync(string novelId, int currentChapterNumber)
        {
            try
            {
                return await _collection.Find(x => x.novel_id == novelId && x.chapter_number < currentChapterNumber)
                                        .SortByDescending(x => x.chapter_number).FirstOrDefaultAsync();
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        /// <summary>
        /// Lấy chương kế tiếp
        /// </summary>
        public async Task<ChapterEntity?> GetNextAsync(string novelId, int currentChapterNumber)
        {
            try
            {
                return await _collection.Find(x => x.novel_id == novelId && x.chapter_number > currentChapterNumber)
                                        .SortBy(x => x.chapter_number).FirstOrDefaultAsync();
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        /// <summary>
        /// Đánh lại số thứ tự chương sau khi chỉnh sửa
        /// </summary>
        public async Task RenumberAsync(string novelId)
        {
            try
            {
                var filter = Builders<ChapterEntity>.Filter.Eq(c => c.novel_id, novelId) &
                             Builders<ChapterEntity>.Filter.Eq(c => c.is_draft, false) &
                             Builders<ChapterEntity>.Filter.Eq(c => c.is_public, true);
                var chapters = await _collection.Find(filter).SortBy(c => c.chapter_number).ToListAsync();

                int number = 1;
                var updates = new List<WriteModel<ChapterEntity>>();
                foreach (var chapter in chapters)
                {
                    if (chapter.chapter_number != number)
                    {
                        chapter.chapter_number = number;
                        var update = Builders<ChapterEntity>.Update.Set(c => c.chapter_number, number);
                        updates.Add(new UpdateOneModel<ChapterEntity>(
                            Builders<ChapterEntity>.Filter.Eq(c => c.id, chapter.id), update));
                    }
                    number++;
                }

                if (updates.Any())
                    await _collection.BulkWriteAsync(updates);
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        /// <summary>
        /// Lấy tổng số chapter đã public của một truyện
        /// </summary>
        public async Task<int> CountPublishedAsync(string novelId)
        {
            try
            {
                var filter = Builders<ChapterEntity>.Filter.And(
                    Builders<ChapterEntity>.Filter.Eq(c => c.novel_id, novelId),
                    Builders<ChapterEntity>.Filter.Eq(c => c.is_public, true),
                    Builders<ChapterEntity>.Filter.Eq(c => c.is_draft, false)
                );

                return (int)await _collection.CountDocumentsAsync(filter);
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        /// <summary>
        /// Tăng số lượng comment
        /// </summary>
        public async Task<bool> IncrementCommentsAsync(string chapterId)
        {
            try
            {
                var update = Builders<ChapterEntity>.Update.Inc(x => x.comment_count, 1);
                var result = await _collection.UpdateOneAsync(x => x.id == chapterId, update);
                return result.ModifiedCount > 0;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        /// <summary>
        /// Giảm số lượng comment
        /// </summary>
        public async Task<bool> DecrementCommentsAsync(string chapterId)
        {
            try
            {
                var update = Builders<ChapterEntity>.Update.Inc(x => x.comment_count, -1);
                var result = await _collection.UpdateOneAsync(x => x.id == chapterId, update);
                return result.ModifiedCount > 0;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        /// <summary>
        /// Phát hành các chương theo lịch đã lên
        /// </summary>
        public async Task<int> ReleaseScheduledAsync()
        {
            try
            {
                // Mốc ngày hiện tại UTC
                var nowVN = DateTime.UtcNow.AddHours(7); // Giờ Việt Nam
                var nowTicks = nowVN.Ticks;

                var filter = Builders<ChapterEntity>.Filter.And(
                    Builders<ChapterEntity>.Filter.Lte(c => c.scheduled_at, nowTicks),
                    Builders<ChapterEntity>.Filter.Eq(c => c.is_public, false),
                    Builders<ChapterEntity>.Filter.Eq(c => c.is_draft, false)
                );

                var chaptersToRelease = await _collection.Find(filter).ToListAsync();
                if (!chaptersToRelease.Any())
                    return 0;

                int updatedCount = 0;

                var novelChapterMap = new Dictionary<string, int>();

                foreach (var chapter in chaptersToRelease.OrderBy(c => c.scheduled_at))
                {
                    if (!novelChapterMap.ContainsKey(chapter.novel_id))
                    {
                        var lastChapter = await _collection.Find(c =>
                            c.novel_id == chapter.novel_id && c.is_public)
                            .SortByDescending(c => c.chapter_number)
                            .FirstOrDefaultAsync();

                        novelChapterMap[chapter.novel_id] = (lastChapter?.chapter_number ?? 0);
                    }

                    novelChapterMap[chapter.novel_id]++;
                    chapter.chapter_number = novelChapterMap[chapter.novel_id];
                    chapter.is_public = true;
                    chapter.is_lock = false;
                    chapter.updated_at = DateTime.UtcNow.Ticks;

                    await _collection.ReplaceOneAsync(c => c.id == chapter.id, chapter);
                    updatedCount++;
                }

                return updatedCount;
            }
            catch
            {
                throw new InternalServerException("Error while releasing scheduled chapters.");
            }
        }

        public async Task IncreaseViewCountAsync(string chapterId)
        {
            var filter = Builders<ChapterEntity>.Filter.Eq(c => c.id, chapterId);
            var update = Builders<ChapterEntity>.Update.Inc(c => c.total_chapter_views, 1);

            await _collection.UpdateOneAsync(filter, update);
        }
    }
}
