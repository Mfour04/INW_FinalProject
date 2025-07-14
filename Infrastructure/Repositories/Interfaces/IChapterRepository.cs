using Domain.Entities.System;
using Domain.Entities;

namespace Infrastructure.Repositories.Interfaces
{
    public interface IChapterRepository
    {
        // CRUD
        Task<ChapterEntity> CreateAsync(ChapterEntity entity);
        Task<ChapterEntity> UpdateAsync(ChapterEntity entity);
        Task<bool> DeleteAsync(string id);

        // Get by ID
        Task<ChapterEntity> GetByIdAsync(string chapterId);
        Task<List<ChapterEntity>> GetChaptersByIdsAsync(List<string> chapterIds);

        // Get all chapters
        Task<List<ChapterEntity>> GetAllAsync(FindCreterias creteriass);
        Task<List<ChapterEntity>> GetAllByNovelIdAsync(string novelId);
        Task<(List<ChapterEntity> Chapters, int TotalChapters, int TotalPages)> GetPagedByNovelIdAsync(string novelId, ChapterFindCreterias creterias, List<SortCreterias> sortCreterias);

        // Get chapter IDs
        Task<List<string>> GetIdsByNovelIdAsync(string novelId);
        Task<List<string>> GetFreeIdsByNovelIdAsync(string novelId);

        // Filtered chapter lists
        Task<List<ChapterEntity>> GetSequentialByNovelIdAsync(string novelId);
        Task<List<ChapterEntity>> GetFreeByNovelIdAsync(string novelId);
        Task<List<ChapterEntity>> GetPublishedByNovelIdAsync(string novelId);

        // Navigational & ordering
        Task<ChapterEntity?> GetLastPublishedAsync(string novelId);
        Task<ChapterEntity?> GetPreviousAsync(string novelId, int currentChapterNumber);
        Task<ChapterEntity?> GetNextAsync(string novelId, int currentChapterNumber);
        Task RenumberAsync(string novelId);

        // Counters & stats
        Task<int> CountPublishedAsync(string novelId);
        Task<bool> IncrementCommentsAsync(string novelId);
        Task<bool> DecrementCommentsAsync(string novelId);
        Task<int> ReleaseScheduledAsync();
        Task IncreaseViewCountAsync(string chapterId);
    }
}
