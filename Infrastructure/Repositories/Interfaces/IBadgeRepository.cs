using Domain.Entities;
using Domain.Entities.System;

namespace Infrastructure.Repositories.Interfaces
{
    public interface IBadgeRepository
    {
        Task<List<BadgeEntity>> GetAllAsync(FindCreterias creterias, List<SortCreterias> sortCreterias);
        Task<List<BadgeEntity>> GetAllWithoutPagingAsync();
        Task<BadgeEntity> GetByIdAsync(string id);
        Task<BadgeEntity> CreateAsync(BadgeEntity entity);
        Task<bool> UpdateAsync(string id, BadgeEntity entity);
        Task<bool> DeleteAsync(string id);
    }
}