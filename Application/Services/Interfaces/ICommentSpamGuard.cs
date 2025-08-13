using Shared.Contracts.Response;

namespace Application.Services.Interfaces
{
    public interface ICommentSpamGuard
    {
        Task<ApiResponse?> CheckSpamAsync(string userId, string novelId, string? chapterId, string content);
    }

}