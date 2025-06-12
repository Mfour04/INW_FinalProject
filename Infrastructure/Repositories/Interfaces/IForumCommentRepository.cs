using Domain.Entities;
using Domain.Entities.System;

namespace Infrastructure.Repositories.Interfaces
{
    public interface IForumCommentRepository
    {
        Task<List<ForumCommentEntity>> GetAllByPostIdAsync(string postId, FindCreterias creterias, List<SortCreterias> sortCreterias);
        Task<ForumCommentEntity> GetByIdAsync(string id);
        Task<ForumCommentEntity> CreateAsync(ForumCommentEntity entity);
        Task<bool> UpdateAsync(string id, ForumCommentEntity entity);
        Task<bool> DeleteAsync(string id);
    }
}