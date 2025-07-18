using AutoMapper;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Chapter;
using Shared.Contracts.Response.Notification;
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
        private readonly IMapper _mapper;
        public GetNotificationByUserIdHandler(INotificationRepository notificationRepository, IMapper mapper)
        {
            _notificationRepository = notificationRepository;
            _mapper = mapper;
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
            var notificationUserResponse = _mapper.Map<List<NotificationReponse>>(notificationUser);
            return new ApiResponse
            {
                Success = true,
                Message = "Get Notification By UserId Successfully",
                Data = notificationUserResponse
            };
        }
    }
}
