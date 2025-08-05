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
        private readonly IMongoCollection<ChapterContentEmbeddingEntity> _chapterContentCollection;
        public OpenAIRepository(MongoDBHelper mongoDBHelper)
        {
            _userCollection = mongoDBHelper.GetCollection<UserEmbeddingEntity>("user_embedding");
            _novelCollection = mongoDBHelper.GetCollection<NovelEmbeddingEntity>("novel_embedding");
            _chapterContentCollection = mongoDBHelper.GetCollection<ChapterContentEmbeddingEntity>("chapter_content_embeddings");
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

        public async Task SaveListNovelEmbeddingAsync(
            List<string> novelIds,
            List<List<float>> vectors,
            List<List<string>> tagsList)
        {
            // Bảo vệ: Kiểm tra kích thước list đầu vào có đồng bộ không
            if (novelIds.Count != vectors.Count || novelIds.Count != tagsList.Count)
            {
                throw new ArgumentException("Số lượng novelIds, vectors, và tagsList phải bằng nhau.");
            }

            var currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            // Ghép từng phần tử theo chỉ mục, tránh lỗi index
            var entities = novelIds
                .Select((novelId, index) => new NovelEmbeddingEntity
                {
                    novel_id = novelId,
                    vector_novel = vectors[index],
                    updated_at = currentTimestamp
                })
                .ToList();

            // Upsert vào MongoDB
            var models = new List<WriteModel<NovelEmbeddingEntity>>();

            foreach (var entity in entities)
            {
                var filter = Builders<NovelEmbeddingEntity>.Filter.Eq(e => e.novel_id, entity.novel_id);
                var update = Builders<NovelEmbeddingEntity>.Update
                    .Set(e => e.vector_novel, entity.vector_novel)
                    .Set(e => e.updated_at, entity.updated_at);

                var upsert = new UpdateOneModel<NovelEmbeddingEntity>(filter, update) { IsUpsert = true };
                models.Add(upsert);
            }

            if (models.Any())
            {
                await _novelCollection.BulkWriteAsync(models);
            }
        }

        public async Task<List<string>> GetExistingNovelEmbeddingIdsAsync(List<string> novelIds)
        {
            var filter = Builders<NovelEmbeddingEntity>.Filter.In(e => e.novel_id, novelIds);
            var result = await _novelCollection.Find(filter).Project(e => e.novel_id).ToListAsync();
            return result;
        }

        public async Task DeleteUserEmbeddingAsync(string userId)
        {
            var filter = Builders<UserEmbeddingEntity>.Filter.Eq(x => x.user_id, userId);
            await _userCollection.DeleteOneAsync(filter);

        }

        public async Task DeleteNovelEmbeddingAsync(string novelId)
        {
            var filter = Builders<NovelEmbeddingEntity>.Filter.Eq(x => x.novel_id, novelId);
            await _novelCollection.DeleteOneAsync(filter);
        }

        public async Task<List<(string NovelId, float Score)>> GetSimilarNovelsAsync(List<float> inputVector, int topN, string excludeNovelId = null)
        {
            try
            {
                var allEmbeddings = await _novelCollection
                    .Find(Builders<NovelEmbeddingEntity>.Filter.Ne(e => e.novel_id, excludeNovelId))
                    .ToListAsync();

                var results = allEmbeddings
                    .Where(x => x.vector_novel != null && x.vector_novel.Count == inputVector.Count)
                    .Select(x => (
                        NovelId: x.novel_id,
                        Score: (float)SystemHelper.CalculateCosineSimilarity(inputVector, x.vector_novel)
                    ))
                    .OrderByDescending(x => x.Score)
                    .Where(x => x.Score >= 0.5)
                    .Take(topN)
                    .ToList();

                return results;
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu cần
                throw new Exception("Lỗi khi lấy các tiểu thuyết tương tự", ex);
            }
        }

        public async Task SaveChapterContentEmbeddingAsync(ChapterContentEmbeddingEntity embedding)
        {
            try
            {
                var filter = Builders<ChapterContentEmbeddingEntity>.Filter.Eq(e => e.chapter_id, embedding.chapter_id);

                var options = new ReplaceOptions { IsUpsert = true };

                await _chapterContentCollection.ReplaceOneAsync(filter, embedding, options);
            }
            catch (Exception ex)
            {
                throw new Exception("Error when save embedding", ex);
            }
        }

        public async Task<bool> ChapterContentEmbeddingExistsAsync(string chapterId)
        {
            var filter = Builders<ChapterContentEmbeddingEntity>.Filter.Eq(e => e.chapter_id, chapterId);
            var count = await _chapterContentCollection.CountDocumentsAsync(filter);
            return count > 0;
        }

        public async Task<List<ChapterContentEmbeddingEntity>> GetAllChapterContentEmbedding()
        {
            return await _chapterContentCollection.Find(_ => true).ToListAsync();
        }

        /// <summary>
        /// End Embedding methods
        /// </summary>
    }
}
