using Domain.Entities;

namespace Infrastructure.Repositories.Interfaces
{
    public interface INovelViewTrackingRepository
    {
        Task<NovelViewTrackingEntity?> FindByUserAndNovelAsync(string userId, string novelId);
        Task<NovelViewTrackingEntity> CreateViewTrackingAsync(NovelViewTrackingEntity entity);
        Task<NovelViewTrackingEntity> UpdateViewTrackingAsync(NovelViewTrackingEntity entity);
    }
}
