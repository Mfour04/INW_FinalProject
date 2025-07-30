using Domain.Entities;
using Infrastructure.InwContext;
using Infrastructure.Repositories.Interfaces;
using MongoDB.Driver;
using Shared.Exceptions;

namespace Infrastructure.Repositories.Implements
{
    public class AuthorEarningRepository : IAuthorEarningRepository
    {
        private readonly IMongoCollection<AuthorEarningEntity> _collection;

        public AuthorEarningRepository(MongoDBHelper mongoDBHelper)
        {
            mongoDBHelper.CreateCollectionIfNotExistsAsync("author_earning").Wait();
            _collection = mongoDBHelper.GetCollection<AuthorEarningEntity>("author_earning");
        }

        public async Task<AuthorEarningEntity> AddAsync(AuthorEarningEntity entity)
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

        public async Task<List<AuthorEarningEntity>> GetByAuthorIdAsync(string authorId, long startTicks, long endTicks)
        {
            try
            {
                var filterBuilder = Builders<AuthorEarningEntity>.Filter;

                var filter = filterBuilder.Eq(x => x.author_id, authorId);

                if (startTicks > 0)
                    filter &= filterBuilder.Gte(x => x.created_at, startTicks);

                if (endTicks > 0)
                    filter &= filterBuilder.Lte(x => x.created_at, endTicks);

                return await _collection.Find(filter).ToListAsync();

            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<int> GetTotalEarningsAsync(string authorId, long startTicks, long endTicks)
        {
            try
            {
                var earnings = await GetByAuthorIdAsync(authorId, startTicks, endTicks);
                return earnings.Sum(x => x.amount);
            }
            catch
            {
                throw new InternalServerException();
            }
        }
    }
}