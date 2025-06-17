using Domain.Entities;

namespace Infrastructure.Repositories.Interfaces
{
    public interface IBadgeProgressRepository
    {
        Task<bool> ExistsAsync(string userId, string badgeId);
        Task CreateManyAsync(List<BadgeProgressEntity> progresses);
        Task<List<BadgeProgressEntity>> GetByUserIdAsync(string userId);
        Task<List<BadgeProgressEntity>> GetCompletedByUserIdAsync(string userId);
    }
}