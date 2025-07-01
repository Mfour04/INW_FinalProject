using Domain.Entities;
using Domain.Entities.System;
using Infrastructure.InwContext;
using Infrastructure.Repositories.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;
using Shared.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public async Task<ChapterEntity> CreateChapterAsync(ChapterEntity entity)
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

        public async Task<bool> DeleteChapterAsync(string id)
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

        public async Task<List<ChapterEntity>> GetAllChapterAsync(FindCreterias creterias)
        {
            try
            {
                var builder = Builders<ChapterEntity>.Filter;
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

        public async Task<ChapterEntity> GetByChapterIdAsync(string chapterId)
        {
            try
            {
                var result = await _collection.Find(x => x.id == chapterId).FirstOrDefaultAsync();
                return result;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<List<ChapterEntity>> GetChapterByChapterIdAsync(List<string> chapterIds)
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

        public async Task<List<ChapterEntity>> GetChaptersByNovelIdAsync(string novelId)
        {
            try
            {
                var filter = Builders<ChapterEntity>.Filter.Eq(x => x.novel_id, novelId);
                var sort = Builders<ChapterEntity>.Sort.Ascending(x => x.chapter_number);

                return await _collection.Find(filter).Sort(sort).ToListAsync();
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<List<ChapterEntity>> GetFreeChaptersByNovelIdAsync(string novelId)
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

        public async Task<ChapterEntity?> GetLastPublishedChapterAsync(string novelId)
        {
            try
            {
                var filter = Builders<ChapterEntity>.Filter.Eq(c => c.novel_id, novelId) &
                             Builders<ChapterEntity>.Filter.Eq(c => c.is_draft, false) &
                             Builders<ChapterEntity>.Filter.Eq(c => c.is_public, true);

                return await _collection.Find(filter)
                           .SortByDescending(c => c.chapter_number)
                           .FirstOrDefaultAsync();
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<List<ChapterEntity>> GetPublishedChapterByNovelIdAsync(string novelId)
        {
            try
            {
                var filter = Builders<ChapterEntity>.Filter.Eq(c => c.novel_id, novelId) &
                             Builders<ChapterEntity>.Filter.Eq(c => c.is_draft, false) &
                             Builders<ChapterEntity>.Filter.Eq(c => c.is_public, true);

                var sort = Builders<ChapterEntity>.Sort.Ascending(c => c.chapter_number);
                return await _collection.Find(filter).Sort(sort).ToListAsync();
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<int> ReleaseScheduledChaptersAsync()
        {
            try
            {
                var currentTicks = DateTime.UtcNow.Ticks;
                var filter = Builders<ChapterEntity>.Filter.And(
                             Builders<ChapterEntity>.Filter.Eq(c => c.scheduled_at, currentTicks) &
                             Builders<ChapterEntity>.Filter.Eq(c => c.is_public, false) &
                             Builders<ChapterEntity>.Filter.Eq(c => c.is_draft, false)
                );

                var update = Builders<ChapterEntity>.Update.Set(c => c.is_public, true);
                var result = await _collection.UpdateManyAsync(filter, update);

                return (int)result.ModifiedCount;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task RenumberChaptersAsync(string novelId)
        {
            try
            {
                var filter = Builders<ChapterEntity>.Filter.Eq(c => c.novel_id, novelId) &
                 Builders<ChapterEntity>.Filter.Eq(c => c.is_draft, false) &
                 Builders<ChapterEntity>.Filter.Eq(c => c.is_public, true);

                var chapters = await _collection.Find(filter)
                                                .SortBy(c => c.chapter_number)
                                                .ToListAsync();

                int number = 1;
                var updates = new List<WriteModel<ChapterEntity>>();
                foreach (var chapter in chapters)
                {
                    if (chapter.chapter_number != number)
                    {
                        chapter.chapter_number = number;
                        var update = Builders<ChapterEntity>.Update.Set(c => c.chapter_number, number);
                        var updateOne = new UpdateOneModel<ChapterEntity>(
                            Builders<ChapterEntity>.Filter.Eq(c => c.id, chapter.id), update);

                        updates.Add(updateOne);
                    }
                }
                if (updates.Any())
                {
                    await _collection.BulkWriteAsync(updates);
                }
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<ChapterEntity> UpdateChapterAsync(ChapterEntity entity)
        {
            try
            {
                entity.updated_at = DateTime.UtcNow.Ticks;
                var filter = Builders<ChapterEntity>.Filter.Eq(x => x.id, entity.id);
                var result = await _collection.ReplaceOneAsync(filter, entity);

                return entity;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        /// <summary>
        /// Lấy danh sách ID của tất cả các chương trong một truyện.
        /// </summary>
        public async Task<List<string>> GetChapterIdsByNovelIdAsync(string novelId)
        {
            try
            {
                var filter = Builders<ChapterEntity>.Filter.Eq(c => c.novel_id, novelId);
                return await _collection.Find(filter).Project(c => c.id).ToListAsync();
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        /// <summary>
        /// Lấy danh sách ID của các chương miễn phí trong một truyện.
        /// </summary>
        public async Task<List<string>> GetFreeChapterIdsByNovelIdAsync(string novelId)
        {
            try
            {
                var filter = Builders<ChapterEntity>.Filter.And(
                    Builders<ChapterEntity>.Filter.Eq(c => c.novel_id, novelId),
                    Builders<ChapterEntity>.Filter.Eq(c => c.is_paid, false)
                );
                return await _collection.Find(filter).Project(c => c.id).ToListAsync();
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<int> GetTotalPublicChaptersAsync(string novelId)
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
    }
}
