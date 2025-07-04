using Domain.Entities;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Notification.Command
{
    public class SendNotificationToUserCommand: IRequest<ApiResponse>
    {
        public string UserId { get; set; }
        public string Message { get; set; }
        public string Type { get; set; }
    }

    public class SendNotificationToUserHandler : IRequestHandler<SendNotificationToUserCommand, ApiResponse>
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly INotificationService _notificationService;
        public SendNotificationToUserHandler(INotificationRepository notificationRepository, INotificationService notificationService)
        {
            _notificationRepository = notificationRepository;
            _notificationService = notificationService;
        }
        public async Task<ApiResponse> Handle(SendNotificationToUserCommand request, CancellationToken cancellationToken)
        {
            var notification = new NotificationEntity
            {
                id = SystemHelper.RandomId(),
                user_id = request.UserId,
                message = request.Message,
                type = request.Type,
                created_at = DateTime.UtcNow.Ticks
            };

            // Lưu vào DB
            await _notificationRepository.CreateAsync(notification);

            // Gửi thông báo đến client thông qua SignalR
            await _notificationService.SendNotificationAsync(request.UserId, request.Message, request.Type);
            return new ApiResponse
            {
                Success = true,
                Message = "Send notification to user successfully",
                Data = notification
            };
        }
    }
}
