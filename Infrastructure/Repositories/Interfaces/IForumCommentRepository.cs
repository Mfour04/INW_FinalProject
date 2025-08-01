using Domain.Entities;
using Domain.Entities.System;

namespace Infrastructure.Repositories.Interfaces
{
    public interface IForumCommentRepository
    {
        Task<List<ForumCommentEntity>> GetRootCommentsByPostIdAsync(string postId, FindCreterias creterias, List<SortCreterias> sortCreterias);
        Task<ForumCommentEntity> GetByIdAsync(string id);
        Task<List<ForumCommentEntity>> GetRepliesByCommentIdAsync(string parentId, FindCreterias creterias, List<SortCreterias> sortCreterias);
        Task<ForumCommentEntity> CreateAsync(ForumCommentEntity entity);
        Task<bool> UpdateAsync(string id, ForumCommentEntity entity);
        Task<bool> DeleteAsync(string id);
    }
}