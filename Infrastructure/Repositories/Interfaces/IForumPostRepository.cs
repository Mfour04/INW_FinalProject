using Domain.Entities;
using Domain.Entities.System;

namespace Infrastructure.Repositories.Interfaces
{
    public interface IForumPostRepository
    {
        Task<List<ForumPostEntity>> GetAllAsync(FindCreterias creterias, List<SortCreterias> sortCreterias);
        Task<ForumPostEntity> GetByIdAsync(string id);
        Task<ForumPostEntity> CreateAsync(ForumPostEntity entity);
        Task<bool> UpdateAsync(string id, ForumPostEntity entity);
        Task<bool> DeleteAsync(string id);
        Task<bool> IncrementLikesAsync(string id);
        Task<bool> DecrementLikesAsync(string id);
        Task<bool> IncrementCommentsAsync(string id);
        Task<bool> DecrementCommentsAsync(string id, int count);
    }
}