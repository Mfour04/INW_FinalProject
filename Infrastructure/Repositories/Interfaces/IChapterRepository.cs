using Domain.Entities.System;
using Domain.Entities;

namespace Infrastructure.Repositories.Interfaces
{
    public interface IChapterRepository
    {
        Task<List<ChapterEntity>> GetAllChapterAsync(FindCreterias creteriass);
        Task<ChapterEntity> GetByChapterIdAsync(string novelId);
        Task<ChapterEntity> CreateChapterAsync(ChapterEntity entity);
        Task<ChapterEntity> UpdateChapterAsync(ChapterEntity entity);
        Task<bool> DeleteChapterAsync(string id);
        Task<List<ChapterEntity>> GetChaptersByNovelIdAsync(string novelId);
        Task<List<ChapterEntity>> GetFreeChaptersByNovelIdAsync(string novelId);
        Task<List<ChapterEntity>> GetChapterByChapterIdAsync(List<string> chapterIds);
        Task RenumberChaptersAsync(string novelId);
        Task<ChapterEntity?> GetLastPublishedChapterAsync(string novelId);
        Task<List<ChapterEntity>> GetPublishedChapterByNovelIdAsync(string novelId);
        Task<List<string>> GetChapterIdsByNovelIdAsync(string novelId);
        Task<List<string>> GetFreeChapterIdsByNovelIdAsync(string novelId);
        Task<int> ReleaseScheduledChaptersAsync();
        Task<int> GetTotalPublicChaptersAsync(string novelId);
        Task<bool> IncrementCommentsAsync(string novelId);
        Task<bool> DecrementCommentsAsync(string novelId);
    }
}
