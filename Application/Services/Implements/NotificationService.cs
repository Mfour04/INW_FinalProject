using Application.Services.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Repositories.Interfaces;
using Infrastructure.SignalRHub;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;
using Shared.Helpers;

namespace Application.Services.Implements
{
    public class NotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly INotificationRepository _notificationRepository;
        public NotificationService(IHubContext<NotificationHub> hubContext, INotificationRepository notificationRepository)
        {
            _hubContext = hubContext;
            _notificationRepository = notificationRepository;
        }

        public async Task<IEnumerable<NotificationEntity>> SendNotificationToUsersAsync(
            IEnumerable<string> userIds,
            string message,
            NotificationType type)
        {
            if (userIds == null || !userIds.Any())
                return Enumerable.Empty<NotificationEntity>();

            var nowTicks = TimeHelper.NowTicks;
            var notifications = userIds.Select(uid => new NotificationEntity
            {
                id = SystemHelper.RandomId(),
                user_id = uid,
                type = type,
                message = message,
                is_read = false,
                created_at = nowTicks,
                updated_at = nowTicks
            }).ToList();

            // 1. Save all notifications to the database
            await _notificationRepository.CreateAsync(notifications);

            // 2. Send real-time notifications via SignalR
            var sendTasks = notifications.Select(n =>
                _hubContext.Clients.User(n.user_id).SendAsync("ReceiveNotification", new
                {
                    n.id,
                    n.type,
                    n.message,
                    n.created_at
                })
            );

            await Task.WhenAll(sendTasks);

            // Return the created notifications
            return notifications;
        }
    }
}
