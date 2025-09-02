using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;

namespace Application.Features.ReadingProcess.Command
{
   public class DeleteReadingProcessBulkCommand : IRequest<ApiResponse>
    {
        public List<string> ReadingProcessIds { get; set; } = new();
    }

    public class DeleteReadingProcessBulkCommandHandler
        : IRequestHandler<DeleteReadingProcessBulkCommand, ApiResponse>
    {
        private readonly IReadingProcessRepository _readingProcessRepository;

        public DeleteReadingProcessBulkCommandHandler(IReadingProcessRepository readingProcessRepository)
        {
            _readingProcessRepository = readingProcessRepository;
        }

        public async Task<ApiResponse> Handle(DeleteReadingProcessBulkCommand request, CancellationToken cancellationToken)
        {
            if (request.ReadingProcessIds == null || request.ReadingProcessIds.Count == 0)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "Danh sách ReadingProcessIds trống."
                };
            }

            var ids = request.ReadingProcessIds
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Select(id => id.Trim())
                .Distinct()
                .ToList();

            if (ids.Count == 0)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "Danh sách ReadingProcessIds không hợp lệ."
                };
            }

            var tasks = ids.Select(async id =>
            {
                var deleted = await _readingProcessRepository.DeleteAsync(id);
                return new { Id = id, Deleted = deleted };
            });

            var results = await Task.WhenAll(tasks);

            var deletedItems = results
                .Where(r => r.Deleted != null)
                .Select(r => r.Deleted!)
                .ToList();

            var notFoundIds = results
                .Where(r => r.Deleted == null)
                .Select(r => r.Id)
                .ToList();

            var total = ids.Count;
            var deletedCount = deletedItems.Count;
            var notFoundCount = notFoundIds.Count;

            var message = notFoundCount == 0
                ? "Xóa lịch sử đọc hàng loạt thành công."
                : $"Xóa một phần: {deletedCount}/{total} mục đã bị xóa. {notFoundCount} mục không tìm thấy hoặc đã bị xóa trước đó.";

            return new ApiResponse
            {
                Success = true, 
                Message = message,
                Data = new
                {
                    TotalRequested = total,
                    DeletedCount = deletedCount,
                    NotFoundCount = notFoundCount,
                    NotFoundIds = notFoundIds,
                    DeletedItems = deletedItems
                }
            };
        }
    }
}