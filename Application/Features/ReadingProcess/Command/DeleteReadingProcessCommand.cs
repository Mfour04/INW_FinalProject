using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;

namespace Application.Features.ReadingProcess.Command
{
    public class DeleteReadingProcessCommand : IRequest<ApiResponse>
    {
        public string ReadingProcessId { get; set; }
    }
    public class DeleteReadingProcessCommandHandler : IRequestHandler<DeleteReadingProcessCommand, ApiResponse>
    {
        private readonly IReadingProcessRepository _readingProcessRepository;

        public DeleteReadingProcessCommandHandler(IReadingProcessRepository readingProcessRepository)
        {
            _readingProcessRepository = readingProcessRepository;
        }

        public async Task<ApiResponse> Handle(DeleteReadingProcessCommand request, CancellationToken cancellationToken)
        {
            var deleted = await _readingProcessRepository.DeleteAsync(request.ReadingProcessId);
            if (deleted == null)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "Reading process không tìm thấy hoặc đã xóa"
                };
            }
            return new ApiResponse
            {
                Success = true,
                Message = "Reading process đã được xóa thành công",
                Data = deleted
            };
        }
    }
}
