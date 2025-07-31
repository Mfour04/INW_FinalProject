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
        //Task SaveListNovelEmbeddingAsync(List<string> novelIds, List<float> vector);
        Task SaveListNovelEmbeddingAsync(List<string> novelIds, List<List<float>> vectors, List<List<string>> tagsList);
        Task<List<string>> GetExistingNovelEmbeddingIdsAsync(List<string> novelIds);
        //Task SaveListNovelEmbeddingAsync(List<NovelEmbeddingEntity> embeddings);
        Task DeleteUserEmbeddingAsync(string userId);
        Task DeleteNovelEmbeddingAsync(string novelId);
        /// <summary>
        /// End Embedding methods
        /// </summary>

    }
}
