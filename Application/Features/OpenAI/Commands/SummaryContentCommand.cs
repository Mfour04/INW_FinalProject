using Application.Services.Interfaces;
using MediatR;
using Shared.Contracts.Response;

namespace Application.Features.OpenAI.Commands
{
    public class SummarizeContentCommand : IRequest<ApiResponse>
    {
        public string Content { get; set; }
    }

    public class SummarizeContentHandler : IRequestHandler<SummarizeContentCommand, ApiResponse>
    {
        private readonly IOpenAIService _openAIService;

        public SummarizeContentHandler(IOpenAIService openAIService)
        {
            _openAIService = openAIService;
        }

        public async Task<ApiResponse> Handle(SummarizeContentCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Content))
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "Nội dung không được để trống."
                };
            }

            try
            {
                var summary = await _openAIService.SummarizeContentAsync(request.Content);

                return new ApiResponse
                {
                    Success = true,
                    Message = "Tóm tắt thành công.",
                    Data = summary
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = $"Lỗi khi tóm tắt nội dung: {ex.Message}"
                };
            }
        }
    }
}
