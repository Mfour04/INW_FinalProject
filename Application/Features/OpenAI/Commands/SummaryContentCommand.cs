using Application.Services.Interfaces;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Contracts.Response;
using System.Collections.Concurrent;
using System.Text;

namespace Application.Features.OpenAI.Commands
{
    public class SummarizeContentCommand : IRequest<ApiResponse>
    {
        public string NovelId { get; set; }
    }

    public class SummarizeContentHandler : IRequestHandler<SummarizeContentCommand, ApiResponse>
    {
        private readonly IOpenAIService _openAIService;
        private readonly IChapterRepository _chapterRepository;
        private readonly ILogger<SummarizeContentHandler> _logger;

        // Tunable parameters
        private const int MaxConcurrency = 4;       // number of parallel summarize calls
        private const int GroupSize = 10;           // chapters per group for hierarchical summarization
        private const int MaxGroupSummariesBeforeFinal = 0; // 0==no limit; or set if want extra tiering

        public SummarizeContentHandler(
            IOpenAIService openAIService,
            IChapterRepository chapterRepository,
            ILogger<SummarizeContentHandler> logger)
        {
            _openAIService = openAIService;
            _chapterRepository = chapterRepository;
            _logger = logger;
        }

        public async Task<ApiResponse> Handle(SummarizeContentCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.NovelId))
            {
                return new ApiResponse { Success = false, Message = "NovelId không được để trống." };
            }

            try
            {
                // Prefer repository method that accepts cancellationToken; if not available, you can add it.
                var chapters = await _chapterRepository.GetChaptersByNovelIdAsync(request.NovelId /*, cancellationToken */);
                if (chapters == null || !chapters.Any())
                {
                    return new ApiResponse { Success = false, Message = "Không tìm thấy chapter nào của tiểu thuyết." };
                }

                // sort chapters
                var ordered = chapters.OrderBy(c => c.chapter_number ?? 0).ToList();

                // Parallel summary with throttling and per-chapter error handling
                var semaphore = new SemaphoreSlim(MaxConcurrency);
                var chapterSummariesBag = new ConcurrentBag<(int chapterNumber, string summary)>();
                var tasks = new List<Task>();

                foreach (var ch in ordered)
                {
                    // skip empty content early
                    if (string.IsNullOrWhiteSpace(ch.content)) continue;

                    await semaphore.WaitAsync(cancellationToken);
                    tasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            // OPTIONAL: If your _openAIService supports cancellationToken, pass it
                            // var s = await _openAIService.SummarizeContentAsync(ch.content, cancellationToken);

                            var s = await _openAIService.SummarizeContentAsync(ch.content);
                            if (!string.IsNullOrWhiteSpace(s))
                            {
                                chapterSummariesBag.Add((ch.chapter_number ?? 0, s.Trim()));
                            }
                            else
                            {
                                _logger.LogWarning("Empty summary for chapter {Chapter} of novel {NovelId}", ch.chapter_number, request.NovelId);
                            }
                        }
                        catch (OperationCanceledException)
                        {
                            _logger.LogInformation("Summarization cancelled for chapter {Chapter}", ch.chapter_number);
                            // honour cancellation
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error summarizing chapter {Chapter} of novel {NovelId}", ch.chapter_number, request.NovelId);
                            // decide: skip this chapter summary, continue with others
                        }
                        finally
                        {
                            semaphore.Release();
                        }
                    }, cancellationToken));
                }

                await Task.WhenAll(tasks);

                // Order collected summaries by chapter number
                var chapterSummaries = chapterSummariesBag
                    .OrderBy(t => t.chapterNumber)
                    .Select((t, idx) => $"Chương {t.chapterNumber}: {t.summary}")
                    .ToList();

                if (!chapterSummaries.Any())
                {
                    return new ApiResponse { Success = false, Message = "Không tạo được bất kỳ tóm tắt chương nào." };
                }

                // Hierarchical summarization: group summaries to avoid sending huge prompt
                List<string> groupSummaries = new List<string>();
                if (chapterSummaries.Count <= GroupSize)
                {
                    // small: just combine and final summarize
                    var combined = string.Join("\n\n", chapterSummaries);
                    var finalPrompt = MakeFinalPrompt(combined);
                    var finalSummary = await _openAIService.SummarizeContentAsync(finalPrompt);
                    return new ApiResponse { Success = true, Message = "Tóm tắt tiểu thuyết thành công.", Data = finalSummary };
                }
                else
                {
                    // chunk into groups
                    for (int i = 0; i < chapterSummaries.Count; i += GroupSize)
                    {
                        var chunk = chapterSummaries.Skip(i).Take(GroupSize);
                        var chunkText = string.Join("\n\n", chunk);
                        var prompt = MakeGroupPrompt(i / GroupSize + 1, chunkText);
                        // Optionally do group summaries in parallel but still respect concurrency
                        var groupSummary = await _openAIService.SummarizeContentAsync(prompt);
                        if (!string.IsNullOrWhiteSpace(groupSummary))
                        {
                            groupSummaries.Add(groupSummary.Trim());
                        }
                    }

                    // if groupSummaries still too many, you could run another tier; for simplicity, combine them
                    var combinedGroupSummaries = string.Join("\n\n", groupSummaries.Select((s, idx) => $"Phần {idx + 1}: {s}"));
                    var finalPrompt2 = MakeFinalPrompt(combinedGroupSummaries);
                    var finalSummary = await _openAIService.SummarizeContentAsync(finalPrompt2);

                    return new ApiResponse { Success = true, Message = "Tóm tắt tiểu thuyết thành công.", Data = finalSummary };
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Summarization cancelled for novel {NovelId}", request.NovelId);
                return new ApiResponse { Success = false, Message = "Tác vụ tóm tắt đã bị huỷ." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error summarizing novel {NovelId}", request.NovelId);
                return new ApiResponse { Success = false, Message = $"Lỗi khi tóm tắt nội dung: {ex.Message}" };
            }
        }

        private static string MakeFinalPrompt(string combinedSummaries)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Bạn là một trợ lý tóm tắt chuyên nghiệp. Hãy rút gọn phần sau thành 1 đoạn tóm tắt ngắn (khoảng 5-8 câu), nêu rõ: premise chính, nhân vật chính, xung đột trung tâm, và kết quả/khung hướng kết thúc. Tránh chi tiết phụ. Độ dài: ~120-200 từ. Giữ văn phong trung tính, rõ ràng.");
            sb.AppendLine();
            sb.AppendLine("Nội dung:");
            sb.AppendLine(combinedSummaries);
            return sb.ToString();
        }

        private static string MakeGroupPrompt(int groupIndex, string chunkSummaries)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Bạn là trợ lý tóm tắt. Đây là nhóm tóm tắt chương (nhóm {groupIndex}). Hãy rút gọn mỗi nhóm thành 1 đoạn 3-5 câu, nêu các sự kiện chính, nhân vật chính, and hook cho phần tiếp theo.");
            sb.AppendLine();
            sb.AppendLine(chunkSummaries);
            return sb.ToString();
        }
    }
}
