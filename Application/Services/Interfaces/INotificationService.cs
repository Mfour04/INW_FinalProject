using Domain.Enums;

namespace Application.Services.Interfaces
{
    public interface INotificationService
    {
        Task SendNotificationAsync(string userId, string message, NotificationType type);
    }
}
