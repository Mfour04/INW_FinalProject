using Application.Services.Interfaces;
using Infrastructure.Repositories.Implements;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Helpers;
using System.Text.RegularExpressions;

namespace Application.Features.OpenAI.Commands
{
    public class PlagiarismChapterConent : IRequest<ApiResponse>
    {
        public string Content { get; set; }
        public int PageNumber { get; set; } = 0;
        public int PageSize { get; set; } = 3;
    }
    public class PlagiarismChapterConentHandler : IRequestHandler<PlagiarismChapterConent, ApiResponse>
    {
        private readonly IOpenAIRepository _openAIRepository;
        private readonly IOpenAIService _openAIService;
        private readonly IChapterRepository _chapterRepository;
        private readonly INovelRepository _novelRepository;
        private string StripHtmlTags(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // Loại bỏ toàn bộ tag <...>
            return Regex.Replace(input, @"<\/?[^>]+>", string.Empty)
             .Replace("&nbsp;", " ")
             .Replace("&amp;", "&")
             .Replace("&quot;", "\"")
             .Trim();
        }
        public PlagiarismChapterConentHandler(IOpenAIRepository openAIRepository, IOpenAIService openAIService
            , IChapterRepository chapterRepository, INovelRepository novelRepository)
        {
            _openAIRepository = openAIRepository;
            _openAIService = openAIService;
            _chapterRepository = chapterRepository;
            _novelRepository = novelRepository;
        }

        public async Task<ApiResponse> Handle(PlagiarismChapterConent request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Content))
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "Content is empty"
                };
            }
            var cleanContent = StripHtmlTags(request.Content);

            float SimilarityThreshold = 0.8f;
            int WordsPerChunk = 50;

            var embeddings = await _openAIRepository.GetAllChapterContentEmbedding();
            if (embeddings == null || embeddings.Count < 2)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "Not enough chapters to perform plagiarism check."
                };
            }

            // 2. Lấy nội dung tất cả chapters trong 1 query
            var chapterIds = embeddings.Select(e => e.chapter_id).ToList();
            var chapters = await _chapterRepository.GetChaptersByIdsAsync(chapterIds);
            var chapterDict = chapters.ToDictionary(c => c.id, c => c);
            // 🔹 Lấy novels từ chapters
            var novelIds = chapters.Select(c => c.novel_id).Distinct().ToList();
            var novels = await _novelRepository.GetManyByIdsAsync(novelIds);
            var novelDict = novels.ToDictionary(n => n.id, n => n);
            // 3. Chuẩn bị text cho embedding
            var allTexts = new List<string>();

            // - Input full
            allTexts.Add(cleanContent);

            // - Input chunks
            var inputChunks = ChunkText(cleanContent, WordsPerChunk);
            if (inputChunks.Count == 0 && !string.IsNullOrWhiteSpace(cleanContent))
            {
                inputChunks.Add(cleanContent);
            }
            int inputChunksStartIndex = allTexts.Count;
            allTexts.AddRange(inputChunks);


            // - Chapter chunks
            var chapterChunksMap = new Dictionary<string, (int startIndex, int count, List<float> vectorFull)>();

            foreach (var chapterEmb in embeddings)
            {
                if (!chapterDict.TryGetValue(chapterEmb.chapter_id, out var chapter))
                    continue;

                if (chapterEmb.vector_chapter_content == null ||
                    string.IsNullOrWhiteSpace(chapter.content))
                    continue;

                var chapterChunks = ChunkText(StripHtmlTags(chapter.content), WordsPerChunk);
                int startIndex = allTexts.Count;
                allTexts.AddRange(chapterChunks);

                chapterChunksMap[chapterEmb.chapter_id] =
                    (startIndex, chapterChunks.Count, chapterEmb.vector_chapter_content);
            }

            // 4. Gọi embedding 1 lần duy nhất
            var allEmbeddings = await _openAIService.GetEmbeddingAsync(allTexts);
            if (allEmbeddings == null)
            {
                Console.WriteLine("[Plagiarism] Embedding batch call failed or returned mismatched counts.");
                return new ApiResponse { Success = false, Message = "Failed to generate embeddings for texts." };
            }

            // Validate size
            if (allEmbeddings.Count != allTexts.Count)
            {
                Console.WriteLine($"[Plagiarism] Embedding count mismatch: allEmbeddings={allEmbeddings.Count}, allTexts={allTexts.Count}");
                return new ApiResponse { Success = false, Message = "Embedding result count mismatch." };
            }

            // Debug info (log một vài giá trị để debug nếu cần)
            Console.WriteLine($"[Plagiarism] total texts={allTexts.Count}, inputChunks={inputChunks.Count}, chaptersConsidered={chapterChunksMap.Count}");
            Console.WriteLine($"[Plagiarism] sample vector dimension: {allEmbeddings[0]?.Count}");

            // 5. Lấy embedding input
            var inputEmbedding = allEmbeddings[0];
            var inputChunksEmbeddings = allEmbeddings
                .Skip(inputChunksStartIndex)
                .Take(inputChunks.Count)
                .ToList();

            // 6. So sánh
            var suspectedChapters = new List<object>();

            foreach (var kvp in chapterChunksMap)
            {
                var chapterId = kvp.Key;
                var (startIndex, count, vectorFull) = kvp.Value;

                // So sánh full chapter trước
                var scoreFull = SystemHelper.CalculateCosineSimilarity(inputEmbedding, vectorFull);
                if (scoreFull < SimilarityThreshold)
                    continue;

                var chapterChunksEmbeddings = allEmbeddings.Skip(startIndex).Take(count).ToList();
                var chapterChunks = allTexts.Skip(startIndex).Take(count).ToList();

                var plagiarizedChunks = FindPlagiarizedChunks_PreEmbedded(
                    inputChunks, inputChunksEmbeddings,
                    chapterChunks, chapterChunksEmbeddings,
                    SimilarityThreshold
                );

                var chapter = chapterDict[chapterId];
                var novel = novelDict.ContainsKey(chapter.novel_id) ? novelDict[chapter.novel_id] : null;

                if (plagiarizedChunks.Any()) // chỉ add nếu có match
                {
                    suspectedChapters.Add(new
                    {
                        ChapterId = chapterId,
                        ChapterTitle = chapter.title,
                        NovelId = chapter.novel_id,
                        NovelTitle = novel?.title,
                        NovelSlug = novel?.slug,
                        Similarity = scoreFull,
                        Matches = plagiarizedChunks
                    });
                }

            }

            int totalCount = suspectedChapters.Count;
            int skip = (request.PageNumber - 1) * request.PageSize;
            var pagedResult = suspectedChapters.Skip(skip).Take(request.PageSize).ToList();

            return new ApiResponse
            {
                Success = true,
                Message = "Plagiarism check completed successfully.",
                Data = new
                {
                    InputContentLength = cleanContent.Length,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize,
                    MatchCount = pagedResult.Count,
                    Matches = pagedResult
                }
            };
        }

        private List<string> ChunkText(string text, int wordsPerChunk = 50)
        {
            var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var chunks = new List<string>();

            for (int i = 0; i < words.Length; i += wordsPerChunk)
            {
                var chunk = string.Join(" ", words.Skip(i).Take(wordsPerChunk));
                if (!string.IsNullOrWhiteSpace(chunk))
                    chunks.Add(chunk);
            }

            return chunks;
        }

        private List<object> FindPlagiarizedChunks_PreEmbedded(
    List<string> inputChunks, List<List<float>> inputEmbeddings,
    List<string> chapterChunks, List<List<float>> chapterEmbeddings,
    float threshold)
        {
            var plagiarized = new List<object>();

            if (inputChunks == null || chapterChunks == null || inputEmbeddings == null || chapterEmbeddings == null)
                return plagiarized;

            int inputCount = Math.Min(inputChunks.Count, inputEmbeddings.Count);
            int chapterCount = Math.Min(chapterChunks.Count, chapterEmbeddings.Count);

            for (int i = 0; i < inputCount; i++)
            {
                var embI = inputEmbeddings[i];
                if (embI == null) continue;

                for (int j = 0; j < chapterCount; j++)
                {
                    var embJ = chapterEmbeddings[j];
                    if (embJ == null) continue;

                    // đảm bảo cùng dimension (tùy impl SystemHelper)
                    if (embI.Count != embJ.Count) continue;

                    var score = SystemHelper.CalculateCosineSimilarity(embI, embJ);
                    if (score >= threshold)
                    {
                        plagiarized.Add(new
                        {
                            InputChunkIndex = i,
                            InputChunk = inputChunks[i],
                            MatchedChunkIndex = j,
                            MatchedChunk = chapterChunks[j],
                            Similarity = score
                        });
                    }
                }
            }

            return plagiarized;
        }

    }
}
