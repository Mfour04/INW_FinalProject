using Application.Services.Interfaces;
using Infrastructure.Repositories.Interfaces;
using Shared.Contracts.Response;
using Shared.Helpers;

namespace Application.Services.Implements
{
    public class CommentSpamGuard : ICommentSpamGuard
    {
        private readonly ICommentRepository _commentRepo;

        public CommentSpamGuard(ICommentRepository commentRepo)
        {
            _commentRepo = commentRepo;
        }

        public async Task<ApiResponse?> CheckSpamAsync(string userId, string novelId, string? chapterId, string content)
        {
            if (CommentContentFilter.ContainsBannedContent(content))
                return Fail("Inappropriate comment.");

            if (await _commentRepo.IsDuplicateCommentAsync(userId, novelId, chapterId, content, 5))
                return Fail("Duplicate comment.");

            if (await _commentRepo.IsSpammingTooFrequentlyAsync(userId, 3, 1))
                return Fail("Too many comments. Please wait.");

            return null;
        }

        private ApiResponse Fail(string msg) => new()
        {
            Success = false,
            Message = msg
        };
    }

}