using Domain.Entities;
using Domain.Entities.System;

namespace Infrastructure.Repositories.Interfaces
{
    public interface ICommentRepository
    {
        Task<List<CommentEntity>> GetCommentsByNovelIdAndChapterIdAsync(FindCreterias findCreterias, string novelId, string chapterId = null);
        Task<CommentEntity> GetCommentByIdAsync(string commentId);
        Task<CommentEntity> CreateCommentAsync(CommentEntity entity);
        Task<CommentEntity> UpdateCommentAsync(CommentEntity entity);
        Task<bool> DeleteCommentAsync(string id);
        Task<List<CommentEntity>> GetCommentsByNovelIdAsync(FindCreterias findCreterias, string novelId);
        Task<List<CommentEntity>> GetCommentsByChapterIdAsync(FindCreterias findCreterias, string chapterId);
        Task<List<CommentEntity>> GetRepliesByParentIdAsync(string parentCommentId);
        Task<bool> IsDuplicateCommentAsync(string userId, string novelId, string? chapterId, string content, int withinMinutes);
        Task<bool> IsSpammingTooFrequentlyAsync(string userId, int limit, int withinMinutes);
    }
}
