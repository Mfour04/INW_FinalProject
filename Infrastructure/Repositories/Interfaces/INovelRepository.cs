using Domain.Entities;
using Domain.Entities.System;
using Shared.Contracts.Response.Novel;

namespace Infrastructure.Repositories.Interfaces
{
    public interface INovelRepository
    {
        Task<(List<NovelEntity> Novels, int TotalCount)> GetAllNovelAsync(FindCreterias filter, List<SortCreterias> sort);
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
        Task UpdateHideNovelAsync(string novelId, bool isPublic);
        Task<bool> IncrementCommentsAsync(string novelId);
        Task<bool> DecrementCommentsAsync(string novelId);
        Task<bool> IsSlugExistsAsync(string slug, string? excludeId = null);
        Task<List<NovelEntity>> GetManyByIdsAsync(List<string> ids);
    }
}
