using Application.Services.Interfaces;
using Infrastructure.Repositories.Interfaces;

namespace Application.Services.Implements
{
    public class ChapterHelperService: IChapterHelperService
    {
        private readonly IChapterRepository _chapterRepository;
        private readonly INovelRepository _novelRepository;
        private readonly ICacheService _cache;
        public ChapterHelperService(IChapterRepository chapterRepo, INovelRepository novelRepo, ICacheService cache)
        {
            _chapterRepository = chapterRepo;
            _novelRepository = novelRepo;
            _cache = cache;
        }

        public async Task<string> GetChapterAuthorIdAsync(string chapterId)
        {
            var chapter = await _chapterRepository.GetByIdAsync(chapterId);
            if (chapter == null) return null;

            var novel = await _novelRepository.GetByNovelIdAsync(chapter.novel_id);
            return novel?.author_id;
        }
        //TimeSpan.FromMinutes(30)
        //TimeSpan.FromHours(24)
        public async Task ProcessViewAsync(string chapterId, string userId)
        {
            // Tạo key để theo dõi lượt xem của từng user (hoặc IP) cho mỗi chapter
            var cacheKey = $"chapter_view:{chapterId}:{userId}";

            // Nếu chưa từng xem trong 24h => tăng view
            if (!await _cache.Exists(cacheKey))
            {
                await _chapterRepository.IncreaseViewCountAsync(chapterId);

                // Lưu vào cache 24h để ngăn người dùng spam view
                await _cache.Set(cacheKey, "1", TimeSpan.FromHours(12));
            }
        }
    }
}
