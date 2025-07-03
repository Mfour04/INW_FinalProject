using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.Interfaces
{
    public interface INotificationRepository
    {
        Task<NotificationEntity> CreateAsync(NotificationEntity notification);
        Task<List<NotificationEntity>> GetUserNotificationsAsync(string userId);
        Task MarkAsReadAsync(string notificationId);
        Task DeleteAsync(string notificationId);
        Task DeleteAllAsync(string userId);
        Task DeleteOldReadNotificationsAsync();
    }
}
