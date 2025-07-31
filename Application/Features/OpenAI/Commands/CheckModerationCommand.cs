using Application.Services.Interfaces;
using MediatR;
using Shared.Contracts.Response;

namespace Application.Features.OpenAI.Commands
{
    public class CheckModerationCommand : IRequest<ApiResponse>    
    {
        public string Content { get; set; }
    }
    public class CheckModerationHandler : IRequestHandler<CheckModerationCommand, ApiResponse>
    {
        private readonly IOpenAIService _openAIService;

        public CheckModerationHandler(IOpenAIService openAIService)
        {
            _openAIService = openAIService;
        }

        public async Task<ApiResponse> Handle(CheckModerationCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Content))
                return new ApiResponse { Success = false, Message = "Nội dung không được để trống." };

            var result = await _openAIService.CheckModerationAsync(request.Content);

            // Lọc các yếu tố có score > 0.3 (tuỳ ngưỡng bạn chọn để cảnh báo)
            var sensitive = result.CategoryScores
                .Where(x => x.Value >= 0.3f)
                .Select(x => new
                {
                    category = x.Key,
                    score = x.Value
                })
                .ToList();

            return new ApiResponse
            {
                Success = true,
                Message = sensitive.Any()
                    ? "Nội dung có chứa yếu tố nhạy cảm, vui lòng xem xét trước khi đăng."
                    : "Nội dung hợp lệ.",
                Data = new
                {
                    flagged = result.Flagged,
                    sensitive
                }
            };
        }
    }

}
