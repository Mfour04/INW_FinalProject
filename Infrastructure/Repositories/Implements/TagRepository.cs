using Domain.Entities;
using Infrastructure.InwContext;
using Infrastructure.Repositories.Interfaces;
using MongoDB.Driver;
using Org.BouncyCastle.Crypto;
using Shared.Exceptions;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.Implements
{
    public class TagRepository : ITagRepository
    {
        private readonly IMongoCollection<TagEntity> _collection;
        public TagRepository(MongoDBHelper mongoDBHelper)
        {
            mongoDBHelper.CreateCollectionIfNotExistsAsync("tag").Wait();
            _collection = mongoDBHelper.GetCollection<TagEntity>("tag");
        }
        public async Task<TagEntity> CreateTagAsync(TagEntity entity)
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

        public async Task<bool> DeleteTagAsync(string id)
        {
            try
            {
                var filter = Builders<TagEntity>.Filter.Eq(x => x.id, id);
                var deleted = await _collection.FindOneAndDeleteAsync(filter);

                return deleted != null;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<List<TagEntity>> GetAllTagAsync()
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

        public async Task<List<TagEntity>> GetByNamesAsync(List<string> names)
        {
            try
            {
                var filter = Builders<TagEntity>.Filter.In(x => x.name, names);
                return await _collection.Find(filter).ToListAsync();
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<TagEntity> GetByTagIdAsync(string tagId)
        {
            try
            {
                var result = await _collection.Find(x => x.id == tagId).FirstOrDefaultAsync();
                return result;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<List<TagEntity>> GetTagsByIdsAsync(List<string> ids)
        {
            try
            {
                var filter = Builders<TagEntity>.Filter.In(x => x.id, ids);
                return await _collection.Find(filter).ToListAsync();
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<TagEntity> UpdateTagAsync(TagEntity entity)
        {
            try
            {
                var filter = Builders<TagEntity>.Filter.Eq(x => x.id, entity.id);
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
