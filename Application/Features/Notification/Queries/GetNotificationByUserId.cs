using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Notification.Queries
{
    public class GetNotificationByUserId: IRequest<ApiResponse>
    {
        public string UserId { get; set; }
    }
    public class GetNotificationByUserIdHandler : IRequestHandler<GetNotificationByUserId, ApiResponse>
    {
        private readonly INotificationRepository _notificationRepository;
        public GetNotificationByUserIdHandler(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }
        public async Task<ApiResponse> Handle(GetNotificationByUserId request, CancellationToken cancellationToken)
        {
            var notificationUser = await _notificationRepository.GetUserNotificationsAsync(request.UserId);
            if (notificationUser == null)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "UserId not found"
                };
            }
            return new ApiResponse
            {
                Success = true,
                Message = "Get Notification By UserId Successfully",
                Data = notificationUser
            };
        }
    }
}
