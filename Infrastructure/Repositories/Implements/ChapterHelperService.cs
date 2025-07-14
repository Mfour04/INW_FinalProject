using Infrastructure.Repositories.Interfaces;

namespace Infrastructure.Repositories.Implements
{
    public class ChapterHelperService: IChapterHelperService
    {
        private readonly IChapterRepository _chapterRepository;
        private readonly INovelRepository _novelRepository;

        public ChapterHelperService(IChapterRepository chapterRepo, INovelRepository novelRepo)
        {
            _chapterRepository = chapterRepo;
            _novelRepository = novelRepo;
        }

        public async Task<string> GetChapterAuthorIdAsync(string chapterId)
        {
            var chapter = await _chapterRepository.GetByIdAsync(chapterId);
            if (chapter == null) return null;

            var novel = await _novelRepository.GetByNovelIdAsync(chapter.novel_id);
            return novel?.author_id;
        }
    }
}
