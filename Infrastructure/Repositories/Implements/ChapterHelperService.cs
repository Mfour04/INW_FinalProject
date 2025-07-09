using Infrastructure.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            var chapter = await _chapterRepository.GetByChapterIdAsync(chapterId);
            if (chapter == null) return null;

            var novel = await _novelRepository.GetByNovelIdAsync(chapter.novel_id);
            return novel?.author_id;
        }
    }
}
