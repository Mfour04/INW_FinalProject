using Domain.Entities.OpenAIEntity;
using Domain.Entities;

namespace Infrastructure.Repositories.Interfaces
{
    public interface IOpenAIRepository
    {
        /// <summary>
        /// Embedding methods
        /// </summary>
        Task<UserEmbeddingEntity?> GetUserEmbeddingAsync(string userId);
        Task SaveUserEmbeddingAsync(string userId, List<float> vector);
        Task<NovelEmbeddingEntity?> GetNovelEmbeddingAsync(string novelId);
        Task SaveNovelEmbeddingAsync(string novelId, List<float> vector);
        Task<List<NovelEmbeddingEntity>> GetAllNovelEmbeddingsAsync();
        Task<List<(string NovelId, float Score)>> GetSimilarNovelsAsync(List<float> inputVector, int topN, string excludeNovelId = null);
        //Task SaveListNovelEmbeddingAsync(List<string> novelIds, List<float> vector);
        Task SaveListNovelEmbeddingAsync(List<string> novelIds, List<List<float>> vectors, List<List<string>> tagsList);
        Task<List<string>> GetExistingNovelEmbeddingIdsAsync(List<string> novelIds);
        Task SaveChapterContentEmbeddingAsync(ChapterContentEmbeddingEntity embedding);
        Task DeleteUserEmbeddingAsync(string userId);
        Task DeleteNovelEmbeddingAsync(string novelId);
        //chapter content embedding methods
        Task<bool> ChapterContentEmbeddingExistsAsync(string chapterId);
        Task<List<ChapterContentEmbeddingEntity>> GetAllChapterContentEmbedding();
        Task<ChapterContentEmbeddingEntity> GetChapterContentEmbeddingByIdAsync(string chapterId);
        Task UpdateChapterContentEmbeddingAsync(ChapterContentEmbeddingEntity entity);
        /// <summary>
        /// End Embedding methods
        /// </summary>
        /// Chapter chunk embedding methods

        Task<List<ChapterChunkEmbeddingEntity>> GetChunksByChapterIdAsync(string chapterId);

        Task SaveChapterChunksAsync(string chapterId, List<string> chunkTexts, List<List<float>> chunkEmbeddings, string novelId);
    }
}
