

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
        Task<IEnumerable<NovelFollowerEntity>> GetByNovelIdAsync(string novelId);
    }
}
