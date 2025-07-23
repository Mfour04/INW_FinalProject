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
        Task SaveListNovelEmbeddingAsync(List<string> novelIds, List<float> vector);
        Task<List<string>> GetExistingNovelEmbeddingIdsAsync(List<string> novelIds);

        /// <summary>
        /// End Embedding methods
        /// </summary>

    }
}
