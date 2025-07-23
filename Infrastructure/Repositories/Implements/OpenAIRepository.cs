using Domain.Entities;
using Domain.Entities.OpenAIEntity;
using Infrastructure.InwContext;
using Infrastructure.Repositories.Interfaces;
using MongoDB.Driver;
using Shared.Helpers;

namespace Infrastructure.Repositories.Implements
{
    public class OpenAIRepository : IOpenAIRepository
    {
        private readonly IMongoCollection<UserEmbeddingEntity> _userCollection;
        private readonly IMongoCollection<NovelEmbeddingEntity> _novelCollection;

        public OpenAIRepository(MongoDBHelper mongoDBHelper)
        {
            _userCollection = mongoDBHelper.GetCollection<UserEmbeddingEntity>("user_embedding");
            _novelCollection = mongoDBHelper.GetCollection<NovelEmbeddingEntity>("novel_embedding");
        }
        /// <summary>
        /// Embedding methods
        /// </summary>
        public async Task<UserEmbeddingEntity?> GetUserEmbeddingAsync(string userId)
        {
            return await _userCollection.Find(x => x.user_id == userId).FirstOrDefaultAsync();
        }

        public async Task SaveUserEmbeddingAsync(string userId, List<float> vector)
        {
            var entity = new UserEmbeddingEntity
            {
                user_id = userId,
                vector_user = vector,
                updated_at = TimeHelper.NowUnixTimeSeconds
            };

            var filter = Builders<UserEmbeddingEntity>.Filter.Eq(x => x.user_id, userId);
            await _userCollection.ReplaceOneAsync(filter, entity, new ReplaceOptions { IsUpsert = true });
        }

        public async Task<NovelEmbeddingEntity?> GetNovelEmbeddingAsync(string novelId)
        {
            return await _novelCollection.Find(x => x.novel_id == novelId).FirstOrDefaultAsync();
        }

        public async Task SaveNovelEmbeddingAsync(string novelId, List<float> vector)
        {
            var entity = new NovelEmbeddingEntity
            {
                novel_id = novelId,
                vector_novel = vector,
                updated_at = TimeHelper.NowUnixTimeSeconds
            };

            var filter = Builders<NovelEmbeddingEntity>.Filter.Eq(x => x.novel_id, novelId);
            await _novelCollection.ReplaceOneAsync(filter, entity, new ReplaceOptions { IsUpsert = true });
        }

        public async Task<List<NovelEmbeddingEntity>> GetAllNovelEmbeddingsAsync()
        {
            return await _novelCollection.Find(Builders<NovelEmbeddingEntity>.Filter.Empty).ToListAsync();
        }

        public async Task SaveListNovelEmbeddingAsync(List<string> novelIds, List<float> vector)
        {
            var models = novelIds.Select(novelId =>
            {
                var filter = Builders<NovelEmbeddingEntity>.Filter.Eq(x => x.novel_id, novelId);
                var entity = new NovelEmbeddingEntity
                {
                    novel_id = novelId,
                    vector_novel = vector,
                    updated_at = TimeHelper.NowUnixTimeSeconds
                };
                return new ReplaceOneModel<NovelEmbeddingEntity>(filter, entity) { IsUpsert = true };
            }).ToList();

            await _novelCollection.BulkWriteAsync(models);
        }

        public async Task<List<string>> GetExistingNovelEmbeddingIdsAsync(List<string> novelIds)
        {
            var filter = Builders<NovelEmbeddingEntity>.Filter.In(e => e.novel_id, novelIds);
            var result = await _novelCollection.Find(filter).Project(e => e.novel_id).ToListAsync();
            return result;
        }

        /// <summary>
        /// End Embedding methods
        /// </summary>
    }
}
