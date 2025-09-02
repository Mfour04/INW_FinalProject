using System.Text.RegularExpressions;
using Application.Services.Interfaces;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Helpers;

namespace Application.Features.OpenAI.Commands
{
    public class PlagiarismChapterContentCommand : IRequest<ApiResponse>
    {
        public string Content { get; set; } // nội dung cần kiểm tra (HTML)
        public string NovelId { get; set; } 
    }

    public class PlagiarismChapterContentHandler : IRequestHandler<PlagiarismChapterContentCommand, ApiResponse>
    {
        private readonly IOpenAIService _openAIService;
        private readonly IChapterRepository _chapterRepository;
        private readonly IOpenAIRepository _embeddingRepository;
        private readonly INovelRepository _novelRepository;

        // Base config (from code 2)
        private const int WordsPerChunk = 200;
        private const double SimilarityThreshold = 0.75;

        // Additional stricter signals (from v4)
        private const double SimilarityThresholdChunk = 0.75;
        private const int LiteralNgramPrimary = 8;
        private const int LiteralNgramSecondary = 6;
        private const double LiteralRateClearThreshold = 0.15;
        private const int MinWordsForSemantic = 400;
        private const int PhraseGramSize = 5;
        private const int MinPhraseMatchesForBoost = 2;
        private const double ContentWordOverlapMin = 0.30;
        private const double ContextOnlyLiteralCap = 0.02; // used to avoid context-only hits

        public PlagiarismChapterContentHandler(
            IOpenAIService openAIService,
            IChapterRepository chapterRepository,
            IOpenAIRepository embeddingRepository,
            INovelRepository novelRepository)
        {
            _openAIService = openAIService;
            _chapterRepository = chapterRepository;
            _embeddingRepository = embeddingRepository; // <-- corrected below
            _novelRepository = novelRepository;
        }

        // Fix constructor assignment properly (replace previous block in your project)
        public PlagiarismChapterContentHandler(
            IOpenAIService openAIService,
            IChapterRepository chapterRepository,
            IOpenAIRepository embeddingRepository,
            INovelRepository novelRepository,
            bool _constructorWorkaround = true)
        {
            _openAIService = openAIService;
            _chapterRepository = chapterRepository;
            _embeddingRepository = embeddingRepository;
            _novelRepository = novelRepository;
        }

        public async Task<ApiResponse> Handle(PlagiarismChapterContentCommand request, CancellationToken cancellationToken)
        {
            // 1. Làm sạch HTML từ input
            var cleanInput = StripHtmlTags(request.Content ?? string.Empty);
            if (string.IsNullOrWhiteSpace(cleanInput))
            {
                return new ApiResponse
                {
                    Success = true,
                    Message = "Nội dung trống.",
                    Data = new
                    {
                        InputContentLength = 0,
                        MatchCount = 0,
                        Matches = Array.Empty<object>()
                    }
                };
            }

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
                if (chapterEmbedding.novel_id == request.NovelId)
                {
                    Console.WriteLine($"[PlagiarismCheck] Skipped ChapterId={chapterEmbedding.chapter_id} vì cùng NovelId={request.NovelId}");
                    continue;
                }

                // 5. So sánh input với vector_chapter_content (full chapter)
                var scoreFull = SystemHelper.CalculateCosineSimilarity(inputEmbedding, chapterEmbedding.vector_chapter_content);
                if (scoreFull < SimilarityThreshold)
                    continue; // bỏ qua nếu quá thấp (như code 2)

                // 6. Lấy thông tin chapter & novel
                var chapter = await _chapterRepository.GetByIdAsync(chapterEmbedding.chapter_id);
                if (chapter == null || string.IsNullOrWhiteSpace(chapter.content))
                    continue;

                var novel = await _novelRepository.GetByNovelIdAsync(chapterEmbedding.novel_id);

                // 7. Lấy hoặc tạo chunk-level embeddings
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
                    var cleanChapterContent = StripHtmlTags(chapter.content ?? string.Empty);
                    if (string.IsNullOrWhiteSpace(cleanChapterContent))
                        continue;

                    chapterChunksTexts = SplitText(cleanChapterContent, WordsPerChunk);
                    chapterChunksEmbeddings = await _openAIService.GetEmbeddingAsync(chapterChunksTexts);

                    await _embeddingRepository.SaveChapterChunksAsync(
                        chapterEmbedding.chapter_id,
                        chapterChunksTexts,
                        chapterChunksEmbeddings,
                        chapterEmbedding.novel_id
                    );
                }

                // 8. Compute semantic coverage (how many input chunks match chapter chunks)
                var (coverage, chunkMatches) = ComputeSemanticCoverage(
                    inputChunks, inputChunksEmbeddings,
                    chapterChunksTexts, chapterChunksEmbeddings,
                    SimilarityThresholdChunk
                );

                // 9. Compute literal overlap, content overlap, phrase matches
                var cleanChapterForLiteral = StripHtmlTags(chapter.content ?? string.Empty);
                var literalPrimary = ComputeWeightedLiteralOverlap(cleanInput, cleanChapterForLiteral, LiteralNgramPrimary);
                var literalSecondary = ComputeWeightedLiteralOverlap(cleanInput, cleanChapterForLiteral, LiteralNgramSecondary) * 0.5;
                var literalWeighted = Math.Max(0.0, Math.Min(1.0, literalPrimary + literalSecondary)); // clamp 0..1

                var contentOverlap = ComputeContentWordOverlap(cleanInput, cleanChapterForLiteral);
                var phrase5Matches = CountPhraseMatches(cleanInput, cleanChapterForLiteral, PhraseGramSize);

                // 10. Guard context-only (loại bỏ các trường hợp chỉ giống ngữ cảnh chung)
                if (IsContextOnly(scoreFull, coverage, literalWeighted, contentOverlap, phrase5Matches, cleanInput))
                    continue;

                // 11. Classify
                var classification = Classify(scoreFull, coverage, literalWeighted, contentOverlap, phrase5Matches, cleanInput);

                if (classification == "None")
                    continue;

                // Build plagiarizedChunks in requested shape and exclude empty ones
                var plagiarizedChunks = new List<object>();
                foreach (var m in chunkMatches)
                {
                    if (m.Similarity >= SimilarityThresholdChunk)
                    {
                        var inputIdx = m.InputChunkIndex;
                        var chapIdx = m.ChapterChunkIndex;

                        var inputText = inputChunks.ElementAtOrDefault(inputIdx) ?? string.Empty;
                        var matchedText = chapterChunksTexts.ElementAtOrDefault(chapIdx) ?? string.Empty;

                        // skip empty snippets
                        if (string.IsNullOrWhiteSpace(inputText) || string.IsNullOrWhiteSpace(matchedText))
                            continue;

                        plagiarizedChunks.Add(new
                        {
                            inputChunk = inputText,
                            matchedChunk = matchedText,
                            similarity = Math.Round(m.Similarity, 4)
                        });
                    }
                }

                // If no chunk-level matches, do not show this suspected chapter (per request)
                if (!plagiarizedChunks.Any())
                    continue;

                suspectedChapters.Add(new
                {
                    ChapterId = chapterEmbedding.chapter_id,
                    ChapterTitle = chapter.title,
                    NovelId = chapterEmbedding.novel_id,
                    NovelTitle = novel?.title,
                    NovelSlug = novel?.slug,
                    Similarity = Math.Round(scoreFull, 4),
                    Classification = classification,
                    LiteralWeightedRate = Math.Round(literalWeighted, 4),
                    ContentWordOverlap = Math.Round(contentOverlap, 4),
                    Phrase5MatchCount = phrase5Matches,
                    SemanticCoverage = Math.Round(coverage, 4),
                    Matches = plagiarizedChunks 
                });
            }

            return new ApiResponse
            {
                Success = true,
                Message = suspectedChapters.Any()
                    ? "Đã hoàn tất kiểm tra đạo văn."
                    : "Không phát hiện đạo văn.",
                Data = new
                {
                    InputContentLength = cleanInput.Length,
                    MatchCount = suspectedChapters.Count,
                    Matches = suspectedChapters.ToList()
                }
            };
        }

        // ================= Helper types & functions =================
        private string StripHtmlTags(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            var noTags = Regex.Replace(input, @"<\/?[^>]+>", " ");
            noTags = noTags
                .Replace("&nbsp;", " ")
                .Replace("&amp;", "&")
                .Replace("&quot;", "\"")
                .Replace("&lt;", "<")
                .Replace("&gt;", ">");

            return Regex.Replace(noTags, @"\s+", " ").Trim();
        }

        private List<string> SplitText(string text, int wordsPerChunk)
        {
            var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var chunks = new List<string>();

            for (int i = 0; i < words.Length; i += wordsPerChunk)
            {
                var chunk = string.Join(" ", words.Skip(i).Take(wordsPerChunk));
                if (!string.IsNullOrWhiteSpace(chunk)) chunks.Add(chunk);
            }

            return chunks;
        }
        private class ChunkMatch
        {
            public int InputChunkIndex { get; set; }
            public int ChapterChunkIndex { get; set; }
            public double Similarity { get; set; }
        }

        // Compute semantic coverage as in v4, now returns typed ChunkMatch list
        private static (double coverage, List<ChunkMatch> bestMatches) ComputeSemanticCoverage(
            List<string> inputChunks,
            List<List<float>> inputChunksEmbeddings,
            List<string> chapterChunks,
            List<List<float>> chapterChunksEmbeddings,
            double threshold)
        {
            int hit = 0;
            var bestMatches = new List<ChunkMatch>();

            for (int i = 0; i < inputChunks.Count; i++)
            {
                var vec = inputChunksEmbeddings[i];
                double best = 0.0;
                int bestJ = -1;
                for (int j = 0; j < chapterChunksEmbeddings.Count; j++)
                {
                    var s = SystemHelper.CalculateCosineSimilarity(vec, chapterChunksEmbeddings[j]);
                    if (s > best) { best = s; bestJ = j; }
                }
                if (bestJ >= 0)
                {
                    if (best >= threshold) hit++;
                    bestMatches.Add(new ChunkMatch { InputChunkIndex = i, ChapterChunkIndex = bestJ, Similarity = best });
                }
            }

            double coverage = inputChunks.Count > 0 ? (double)hit / inputChunks.Count : 0.0;
            return (coverage, bestMatches);
        }

        // Mask named entities (basic)
        private static string MaskNamedEntities(string text)
        {
            return Regex.Replace(text, @"\b([A-ZÀ-Ỵ][\p{L}]+(?:\s+[A-ZÀ-Ỵ][\p{L}]+)+)\b", "<NAME>");
        }

        private static List<string> ToNormalizedTokens(string text)
        {
            var lower = text.ToLowerInvariant();
            lower = Regex.Replace(lower, @"[^\p{L}\p{Nd}]+", " ");
            return lower.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        private static List<string> BuildNgrams(List<string> tokens, int n)
        {
            var grams = new List<string>();
            for (int i = 0; i <= tokens.Count - n; i++)
                grams.Add(string.Join(' ', tokens.Skip(i).Take(n)));
            return grams;
        }

        private static double ComputeWeightedLiteralOverlap(string inputRaw, string chapterRaw, int n)
        {
            var input = MaskNamedEntities(inputRaw);
            var chapter = MaskNamedEntities(chapterRaw);

            var a = ToNormalizedTokens(input);
            var b = ToNormalizedTokens(chapter);
            if (a.Count < n || b.Count < n) return 0.0;

            var gramsA = BuildNgrams(a, n);
            var gramsB = new HashSet<string>(BuildNgrams(b, n));
            if (gramsA.Count == 0) return 0.0;

            double matched = 0.0;
            foreach (var g in gramsA) if (gramsB.Contains(g)) matched += 1.0;
            return matched / gramsA.Count;
        }

        private static double ComputeContentWordOverlap(string aRaw, string bRaw)
        {
            var a = ToNormalizedTokens(MaskNamedEntities(aRaw)).Where(t => t.Length >= 5).ToList();
            var b = ToNormalizedTokens(MaskNamedEntities(bRaw)).Where(t => t.Length >= 5).ToList();
            if (!a.Any() || !b.Any()) return 0.0;
            var setA = new HashSet<string>(a);
            var setB = new HashSet<string>(b);
            setA.IntersectWith(setB);
            double inter = setA.Count;
            double denom = Math.Max(1, Math.Min(a.Count, b.Count));
            return inter / denom;
        }

        private static int CountPhraseMatches(string aRaw, string bRaw, int k)
        {
            var a = ToNormalizedTokens(MaskNamedEntities(aRaw));
            var b = ToNormalizedTokens(MaskNamedEntities(bRaw));
            if (a.Count < k || b.Count < k) return 0;
            var gramsA = BuildNgrams(a, k);
            var gramsB = new HashSet<string>(BuildNgrams(b, k));
            int cnt = 0;
            foreach (var g in gramsA) if (gramsB.Contains(g)) cnt++;
            return cnt;
        }

        private static bool IsContextOnly(double simFull, double coverage, double literalRate, double contentOverlap, int phrase5Matches, string cleanInput)
        {
            bool isSmallInput = cleanInput.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length < MinWordsForSemantic;
            if (literalRate <= ContextOnlyLiteralCap &&
                contentOverlap < ContentWordOverlapMin &&
                phrase5Matches < MinPhraseMatchesForBoost)
            {
                if (!isSmallInput && simFull >= 0.95 && coverage >= 0.55)
                    return false;
                return true;
            }
            return false;
        }

        private static string Classify(double simFull, double coverage, double literalRate, double contentOverlap, int phrase5Matches, string cleanInput)
        {
            bool isSmallInput = cleanInput.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length < MinWordsForSemantic;

            if (isSmallInput)
            {
                if (literalRate >= 0.08 || phrase5Matches >= MinPhraseMatchesForBoost || contentOverlap >= (ContentWordOverlapMin + 0.05))
                    return "Clear";
                if (literalRate >= 0.04 || contentOverlap >= ContentWordOverlapMin)
                    return "Related";
                return "None";
            }

            if (literalRate >= LiteralRateClearThreshold) return "Clear";
            if (phrase5Matches >= MinPhraseMatchesForBoost && (simFull >= 0.75 || contentOverlap >= ContentWordOverlapMin))
                return "Clear";
            if (simFull >= 0.93 && coverage >= 0.50 && (literalRate >= 0.08 || contentOverlap >= ContentWordOverlapMin))
                return "Clear";

            if (simFull >= 0.88 && (coverage >= 0.35 || contentOverlap >= ContentWordOverlapMin))
                return "Related";

            return "None";
        }
    }
}
