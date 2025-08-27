using Application.Services.Interfaces;
using Infrastructure.Repositories.Implements;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Shared.Contracts.Response;
using Shared.Helpers;
using System.Text.RegularExpressions;

namespace Application.Features.OpenAI.Commands
{
    public class PlagiarismChapterContentCommand : IRequest<ApiResponse>
    {
        public string Content { get; set; } // nội dung cần kiểm tra (HTML)
    }

    public class PlagiarismChapterContentHandler : IRequestHandler<PlagiarismChapterContentCommand, ApiResponse>
    {
        private readonly IOpenAIService _openAIService;
        private readonly IChapterRepository _chapterRepository;
        private readonly IOpenAIRepository _embeddingRepository;
        private readonly INovelRepository _novelRepository;

        private const int WordsPerChunk = 200;
        private const double SimilarityThreshold = 0.75;

        public PlagiarismChapterContentHandler(
            IOpenAIService openAIService,
            IChapterRepository chapterRepository,
            IOpenAIRepository embeddingRepository,
            INovelRepository novelRepository)
        {
            _openAIService = openAIService;
            _chapterRepository = chapterRepository;
            _embeddingRepository = embeddingRepository;
            _novelRepository = novelRepository;
        }

        public async Task<ApiResponse> Handle(PlagiarismChapterContentCommand request, CancellationToken cancellationToken)
        {
            // 1. Làm sạch HTML từ input
            var cleanInput = StripHtmlTags(request.Content);

            // 2. Chia input thành chunks
            var inputChunks = SplitText(cleanInput, WordsPerChunk);

            // 3. Gọi embedding cho input (full + chunks)
            var allInputTexts = new List<string> { cleanInput };
            allInputTexts.AddRange(inputChunks);

            var inputEmbeddings = await _openAIService.GetEmbeddingAsync(allInputTexts);
            var inputEmbedding = inputEmbeddings[0];
            var inputChunksEmbeddings = inputEmbeddings.Skip(1).ToList();

            // 4. Lấy toàn bộ embedding đã lưu trong DB (full chapter-level)
            var allChapterEmbeddings = await _embeddingRepository.GetAllChapterContentEmbedding();

            var suspectedChapters = new List<object>();

            foreach (var chapterEmbedding in allChapterEmbeddings)
            {
                // 5. So sánh input với vector_chapter_content (full chapter)
                var scoreFull = SystemHelper.CalculateCosineSimilarity(inputEmbedding, chapterEmbedding.vector_chapter_content);
                if (scoreFull < SimilarityThreshold)
                    continue; // bỏ qua nếu quá thấp

                // 6. Lấy thông tin chapter & novel
                var chapter = await _chapterRepository.GetByIdAsync(chapterEmbedding.chapter_id);
                if (chapter == null || string.IsNullOrWhiteSpace(chapter.content))
                    continue; // bỏ qua nếu không có dữ liệu

                var novel = await _novelRepository.GetByNovelIdAsync(chapterEmbedding.novel_id);

                // 7. Lấy hoặc tạo chunk-level embeddings
                var plagiarizedChunks = new List<object>();

                var chapterChunksEntity = await _embeddingRepository.GetChunksByChapterIdAsync(chapterEmbedding.chapter_id);
                List<string> chapterChunksTexts;
                List<List<float>> chapterChunksEmbeddings;

                if (chapterChunksEntity != null && chapterChunksEntity.Any())
                {
                    chapterChunksTexts = chapterChunksEntity.Select(x => x.chunk_text).ToList();
                    chapterChunksEmbeddings = chapterChunksEntity.Select(x => x.vector_chunk_content).ToList();
                }
                else
                {
                    // chưa có thì tạo mới
                    var cleanChapterContent = StripHtmlTags(chapter.content ?? string.Empty);
                    if (string.IsNullOrWhiteSpace(cleanChapterContent))
                        continue;

                    chapterChunksTexts = SplitText(cleanChapterContent, WordsPerChunk);

                    var newChunkEmbeddings = await _openAIService.GetEmbeddingAsync(chapterChunksTexts);
                    chapterChunksEmbeddings = newChunkEmbeddings;

                    await _embeddingRepository.SaveChapterChunksAsync(
                        chapterEmbedding.chapter_id,
                        chapterChunksTexts,
                        chapterChunksEmbeddings,
                        chapterEmbedding.novel_id
                    );
                }

                // 8. So sánh input chunks với chapter chunks
                plagiarizedChunks = FindPlagiarizedChunks_PreEmbedded(
                    inputChunks, inputChunksEmbeddings,
                    chapterChunksTexts, chapterChunksEmbeddings,
                    SimilarityThreshold
                );

                if (plagiarizedChunks.Any())
                {
                    suspectedChapters.Add(new
                    {
                        ChapterId = chapterEmbedding.chapter_id,
                        ChapterTitle = chapter.title,
                        NovelId = chapterEmbedding.novel_id,
                        NovelTitle = novel?.title,
                        NovelSlug = novel?.slug,
                        Similarity = scoreFull,
                        Matches = plagiarizedChunks
                    });
                }
            }

            return new ApiResponse
            {
                Success = true,
                Message = suspectedChapters.Any()
                    ? "Plagiarism check completed."
                    : "No plagiarism detected.",
                Data = new
                {
                    InputContentLength = cleanInput.Length,
                    MatchCount = suspectedChapters.Count,
                    Matches = suspectedChapters.ToList()
                }
            };
        }

        // Helper: loại bỏ thẻ HTML
        private string StripHtmlTags(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            return Regex.Replace(input, @"<\/?[^>]+>", string.Empty)
                        .Replace("&nbsp;", " ")
                        .Replace("&amp;", "&")
                        .Replace("&quot;", "\"")
                        .Trim();
        }

        // Helper: chia text thành các chunk theo số lượng từ
        private List<string> SplitText(string text, int wordsPerChunk)
        {
            var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var chunks = new List<string>();

            for (int i = 0; i < words.Length; i += wordsPerChunk)
            {
                var chunk = string.Join(" ", words.Skip(i).Take(wordsPerChunk));
                chunks.Add(chunk);
            }

            return chunks;
        }

        // Helper: tìm các đoạn có đạo văn
        private List<object> FindPlagiarizedChunks_PreEmbedded(
            List<string> inputChunks,
            List<List<float>> inputChunksEmbeddings,
            List<string> chapterChunks,
            List<List<float>> chapterChunksEmbeddings,
            double threshold)
        {
            var matches = new List<object>();

            for (int i = 0; i < inputChunks.Count; i++)
            {
                var inputVec = inputChunksEmbeddings[i];

                for (int j = 0; j < chapterChunks.Count; j++)
                {
                    var score = SystemHelper.CalculateCosineSimilarity(inputVec, chapterChunksEmbeddings[j]);
                    if (score >= threshold)
                    {
                        matches.Add(new
                        {
                            InputChunk = inputChunks[i],
                            MatchedChunk = chapterChunks[j],
                            Similarity = score
                        });
                    }
                }
            }

            return matches;
        }
    }
}
