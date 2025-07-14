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
                Task<List<ChapterEntity>> GetChapterNmbersByNovelIdAsync(string novelId);
                Task<List<ChapterEntity>> GetFreeChaptersByNovelIdAsync(string novelId);
                Task<List<ChapterEntity>> GetChapterByChapterIdAsync(List<string> chapterIds);
                Task RenumberChaptersAsync(string novelId);
                Task<ChapterEntity?> GetLastPublishedChapterAsync(string novelId);
                Task<List<ChapterEntity>> GetPublishedChapterByNovelIdAsync(string novelId);
                Task<List<string>> GetChapterIdsByNovelIdAsync(string novelId);
                Task<List<string>> GetFreeChapterIdsByNovelIdAsync(string novelId);
                Task<int> ReleaseScheduledChaptersAsync();
                Task<int> GetTotalPublicChaptersAsync(string novelId);
                Task<List<ChapterEntity>> GetAllChapterByNovelId(string novelId);
                Task<bool> IncrementCommentsAsync(string novelId);
                Task<bool> DecrementCommentsAsync(string novelId);
                Task<ChapterEntity?> GetPreviousChapterAsync(string novelId, int currentChapterNumber);
                Task<ChapterEntity?> GetNextChapterAsync(string novelId, int currentChapterNumber);
                Task<(List<ChapterEntity> Chapters, int TotalChapters, int TotalPages)> GetAllChapterIdsByNovelIdAsync(string novelId, ChapterFindCreterias creterias, List<SortCreterias> sortCreterias);
                Task IncreaseViewCountAsync(string chapterId);
        }
}
