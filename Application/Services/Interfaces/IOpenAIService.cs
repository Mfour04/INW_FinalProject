using Shared.Contracts.Response.OpenAI;

namespace Application.Services.Interfaces
{
    public interface IOpenAIService
    {
        //Task<List<float>> GetEmbeddingAsync(List<string> tags);
        Task<List<List<float>>> GetEmbeddingAsync(List<string> inputs);
        Task<ModerationResult> CheckModerationAsync(string input);
    }
}
