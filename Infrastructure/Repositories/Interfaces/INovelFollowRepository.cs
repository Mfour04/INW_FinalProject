using Domain.Entities;
using Domain.Entities.System;

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
        Task<(List<NovelFollowerEntity> NovelFollows, int TotalCount)> GetFollowedNovelsByUserIdAsync(string userId, FindCreterias findCreterias);
        Task<NovelFollowerEntity?> GetByUserAndNovelIdAsync(string userId, string novelId);

    }
}
