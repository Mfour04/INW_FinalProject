using Domain.Entities;
using Infrastructure.Common;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Helpers;
using System;

namespace Application.Features.Novel.Commands
{
    public class HideNovelCommand: IRequest<ApiResponse>
    {
        public string NovelId { get; set; }
    }

    public class HideNovelCommandHandler : IRequestHandler<HideNovelCommand, ApiResponse>
    {
        private readonly INovelRepository _novelRepository;
        private readonly ICurrentUserService _currentUserService;
        public HideNovelCommandHandler(
        INovelRepository novelRepository,
        IPurchaserRepository purchaserRepository,
        INotificationRepository notificationRepository,
        INotificationService notificationService,
        ICurrentUserService currentUserService
        )
        {
            _novelRepository = novelRepository;
            _currentUserService = currentUserService;
        }
        public async Task<ApiResponse> Handle(HideNovelCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userId))
            {
                return new ApiResponse { Success = false, Message = "Unauthorized" };
            }
            var novel = await _novelRepository.GetByNovelIdAsync(request.NovelId);
            if(novel == null)
            {
                return new ApiResponse { Success = false, Message = "Novel not found" };
            }
            if (novel.author_id != userId)
            {
                return new ApiResponse { Success = false, Message = "Forbidden: You are not the author of this novel." };
            }
            await _novelRepository.UpdateLockStatusAsync(request.NovelId, true);

            return new ApiResponse
            {
                Success = true,
                Message = "Novel has been hidden successfully and affected users have been notified."
            };
        }
    }
}
