using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;

namespace Application.Features.Novel.Commands
{
    public class UpdateLockNovelCommand: IRequest<ApiResponse>
    {
        public string NovelId { get; set; }
        public bool IsLocked { get; set; }
    }
    public class UpdateLockNovelHandler : IRequestHandler<UpdateLockNovelCommand, ApiResponse>
    {
        private readonly INovelRepository _novelRepository;
        private readonly ICurrentUserService _currentUserService;
        public UpdateLockNovelHandler(INovelRepository novelRepository, ICurrentUserService currentUserService)
        {
            _novelRepository = novelRepository;
            _currentUserService = currentUserService;
        }
        public async Task<ApiResponse> Handle(UpdateLockNovelCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userId))
            {
                return new ApiResponse { Success = false, Message = "Unauthorized" };
            }
            if (!_currentUserService.IsAdmin())
            {
                return new ApiResponse { Success = false, Message = "Forbidden: Admin role required" };
            }

            var novel = await _novelRepository.GetByNovelIdAsync(request.NovelId);
            if (novel == null)
            {
                return new ApiResponse { Success = false, Message = "Novel not found" };
            }
            await _novelRepository.UpdateLockStatusAsync(request.NovelId, request.IsLocked);
            var action = request.IsLocked ? "locked" : "unlocked";
            return new ApiResponse
            {
                Success = true,
                Message = $"Novel has been {action} successfully and affected users have been notified."
            };
        }
    }
}
