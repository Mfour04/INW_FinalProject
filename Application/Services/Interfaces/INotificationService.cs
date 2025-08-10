using Domain.Entities;
using Domain.Enums;

namespace Application.Services.Interfaces
{
    public interface INotificationService
    {
        Task<IEnumerable<NotificationEntity>> SendNotificationToUsersAsync(IEnumerable<string> userIds, string message, NotificationType type);
    }
}
