using Domain.Entities;
using Domain.Entities.System;

namespace Infrastructure.Repositories.Interfaces
{
    public interface IForumPostRepository
    {
        Task<List<ForumPostEntity>> GetAllForumPostAsync(FindCreterias creterias, List<SortCreterias> sortCreterias);
        Task<ForumPostEntity> CreateForumPostAsync(ForumPostEntity entity);
        Task<bool> UpdateForumPostAsync(string id, ForumPostEntity entity);
        Task<bool> DeleteForumPostAsync(string id);
    }
}