using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;

namespace Application.Features.Notification.Commands
{
    public class MarkNotificationsAsReadCommand: IRequest<ApiResponse>
    {
        public List<string> NotificationIds { get; set; }
    }
    public class MarkNotificationsAsReadHandler : IRequestHandler<MarkNotificationsAsReadCommand, ApiResponse>
    {
        private readonly INotificationRepository _repository;

        public MarkNotificationsAsReadHandler(INotificationRepository repository)
        {
            _repository = repository;
        }

        public async Task<ApiResponse> Handle(MarkNotificationsAsReadCommand request, CancellationToken cancellationToken)
        {
            await _repository.MarkAsReadAsync(request.NotificationIds);
            return new ApiResponse { Success = true, Message = "Thông báo được đánh dấu là đã đọc." };   
        }
    }

}
