using Domain.Entities;
using Domain.Entities.System;

namespace Infrastructure.Repositories.Interfaces
{
    public interface ICommentRepository
    {
        Task<CommentEntity> CreateAsync(CommentEntity entity);
        Task<CommentEntity> UpdateAsync(CommentEntity entity);
        Task<bool> DeleteAsync(string id);
        Task<bool> DeleteRepliesByParentIdAsync(string parentId);
        Task<CommentEntity> GetByIdAsync(string commentId);
        Task<List<CommentEntity>> GetCommentsByNovelIdAsync(string novelId, FindCreterias creterias, List<SortCreterias> sortCreterias);
        Task<List<CommentEntity>> GetCommentsByChapterIdAsync(string novelId, string chapterId, FindCreterias creterias, List<SortCreterias> sortCreterias);
        Task<List<CommentEntity>> GetRepliesByCommentIdAsync(string parentId, FindCreterias creterias, List<SortCreterias> sortCreterias);
        Task<bool> IsDuplicateCommentAsync(string userId, string novelId, string? chapterId, string content, int withinMinutes);
        Task<bool> IsSpammingTooFrequentlyAsync(string userId, int limit, int withinMinutes);
        Task<Dictionary<string, int>> CountRepliesPerCommentAsync(List<string> parentCommentIds);
    }
}
