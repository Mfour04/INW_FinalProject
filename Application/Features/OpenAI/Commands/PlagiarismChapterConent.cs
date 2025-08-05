using Application.Services.Interfaces;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Helpers;

namespace Application.Features.OpenAI.Commands
{
    public class PlagiarismChapterConent: IRequest<ApiResponse>
    {
        public string Content { get; set; }
    }
    public class PlagiarismChapterConentHandler : IRequestHandler<PlagiarismChapterConent, ApiResponse>
    {
        private readonly IOpenAIRepository _openAIRepository;
        private readonly IOpenAIService _openAIService;
        public PlagiarismChapterConentHandler(IOpenAIRepository openAIRepository, IOpenAIService openAIService)
        {
            _openAIRepository = openAIRepository;
            _openAIService = openAIService;
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
            float SimilarityThreshold = 0.9f;
            var embeddings = await _openAIRepository.GetAllChapterContentEmbedding();
            if (embeddings == null || embeddings.Count < 2)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "Not enough chapters to perform plagiarism check."
                };
            }

            var inputEmbeddingList = await _openAIService.GetEmbeddingAsync(new List<string> { request.Content });
            var inputEmbedding = inputEmbeddingList[0];

            var suspectedChapters = new List<object>();

            foreach (var chapterEmb in embeddings)
            {
                if (chapterEmb.vector_chapter_content == null || chapterEmb.vector_chapter_content.Count != inputEmbedding.Count)
                    continue;

                var score = SystemHelper.CalculateCosineSimilarity(inputEmbedding, chapterEmb.vector_chapter_content);
                if (score >= SimilarityThreshold)
                {
                    suspectedChapters.Add(new
                    {
                        ChapterId = chapterEmb.chapter_id,
                        Similarity = score
                    });
                }
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
    }
}
