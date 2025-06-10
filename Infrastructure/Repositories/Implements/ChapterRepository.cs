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
            } catch 
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
    }
}
