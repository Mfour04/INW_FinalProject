using Domain.Entities.System;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
