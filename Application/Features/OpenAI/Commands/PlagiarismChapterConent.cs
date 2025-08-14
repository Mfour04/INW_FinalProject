using Application.Services.Interfaces;
using Infrastructure.Repositories.Implements;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Helpers;

namespace Application.Features.OpenAI.Commands
{
    public class PlagiarismChapterConent : IRequest<ApiResponse>
    {
        public string Content { get; set; }
    }
    public class PlagiarismChapterConentHandler : IRequestHandler<PlagiarismChapterConent, ApiResponse>
    {
        private readonly IOpenAIRepository _openAIRepository;
        private readonly IOpenAIService _openAIService;
        private readonly IChapterRepository _chapterRepository;

        public PlagiarismChapterConentHandler(IOpenAIRepository openAIRepository, IOpenAIService openAIService
            , IChapterRepository chapterRepository)
        {
            _openAIRepository = openAIRepository;
            _openAIService = openAIService;
            _chapterRepository = chapterRepository;
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

            float SimilarityThreshold = 0.6f;
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

            // 3. Chuẩn bị text cho embedding
            var allTexts = new List<string>();

            // - Input full
            allTexts.Add(request.Content);

            // - Input chunks
            var inputChunks = ChunkText(request.Content, WordsPerChunk);
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

                var chapterChunks = ChunkText(chapter.content, WordsPerChunk);
                int startIndex = allTexts.Count;
                allTexts.AddRange(chapterChunks);

                chapterChunksMap[chapterEmb.chapter_id] =
                    (startIndex, chapterChunks.Count, chapterEmb.vector_chapter_content);
            }

            // 4. Gọi embedding 1 lần duy nhất
            var allEmbeddings = await _openAIService.GetEmbeddingAsync(allTexts);

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

                suspectedChapters.Add(new
                {
                    ChapterId = chapterId,
                    Similarity = scoreFull,
                    Matches = plagiarizedChunks
                });
            }

            return new ApiResponse
            {
                Success = true,
                Message = "Plagiarism check completed successfully.",
                Data = new
                {
                    InputContentLength = request.Content.Length,
                    MatchCount = suspectedChapters.Count,
                    Matches = suspectedChapters
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

            for (int i = 0; i < inputChunks.Count; i++)
            {
                for (int j = 0; j < chapterChunks.Count; j++)
                {
                    var score = SystemHelper.CalculateCosineSimilarity(inputEmbeddings[i], chapterEmbeddings[j]);
                    if (score >= threshold)
                    {
                        plagiarized.Add(new
                        {
                            InputChunk = inputChunks[i],
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
