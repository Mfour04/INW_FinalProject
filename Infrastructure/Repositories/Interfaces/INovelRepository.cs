using Domain.Entities;
using Domain.Entities.System;

namespace Infrastructure.Repositories.Interfaces
{
    public interface INovelRepository
    {
        Task<List<NovelEntity>> GetAllNovelAsync(FindCreterias creterias, List<SortCreterias> sortCreterias);
        Task<NovelEntity> GetByNovelIdAsync(string novelId);
        Task<NovelEntity> CreateNovelAsync(NovelEntity entity);
        Task<NovelEntity> UpdateNovelAsync(NovelEntity entity);
        Task<bool> DeleteNovelAsync(string id);
        Task IncrementFollowersAsync(string novelId);
        Task DecrementFollowersAsync(string novelId);
        Task UpdateTotalChaptersAsync(string novelId);
        Task IncreaseTotalViewAsync(string novelId);
        Task<List<NovelEntity>> GetNovelByAuthorId(string authorId);
        Task UpdateLockStatusAsync(string novelId, bool isLocked);
    }
}
