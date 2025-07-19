using Domain.Entities;

namespace Infrastructure.Repositories.Interfaces
{
    public interface INovelFollowRepository
    {
        Task<List<NovelFollowerEntity>> GetAllNovelFollowAsync();
        Task<NovelFollowerEntity> GetByNovelFollowIdAsync(string novelfollowId);
        Task<NovelFollowerEntity> CreateNovelFollowAsync(NovelFollowerEntity entity);
        Task<NovelFollowerEntity> UpdateNovelFollowAsync(NovelFollowerEntity entity);
        Task<bool> DeleteNovelFollowAsync(string id);
        Task<List<NovelFollowerEntity>> GetFollowersByNovelIdAsync(string novelId);
        Task<List<NovelFollowerEntity>> GetFollowedNovelsByUserIdAsync(string userId);
        Task<NovelFollowerEntity?> GetByUserAndNovelIdAsync(string userId, string novelId);

    }
}
