using Application.Features.Notification.Commands;
using Application.Services.Interfaces;
using Domain.Enums;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Helpers;

namespace Application.Features.User.Feature
{
    public class UpdateLockvsUnLockUserCommand: IRequest<ApiResponse>
    {
        public List<string> UserIds { get; set; } = new();
        public bool isBanned { get; set; }   
        public string DurationType { get; set; }
    }
    
    public class UpdateLockvsUnLockUserHandler : IRequestHandler<UpdateLockvsUnLockUserCommand, ApiResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly INotificationRepository _notificationRepository;
        private readonly INotificationService _notificationService;
        private readonly IMediator _mediator;
        public UpdateLockvsUnLockUserHandler(IUserRepository userRepository, ICurrentUserService currentUserService
            , INotificationRepository notificationRepository, INotificationService notificationService, IMediator mediator)
        {
            _userRepository = userRepository;
            _currentUserService = currentUserService;
            _notificationRepository = notificationRepository;
            _notificationService = notificationService;
            _mediator = mediator;
        }
        public async Task<ApiResponse> Handle(UpdateLockvsUnLockUserCommand request, CancellationToken cancellationToken)
        {
            var adminId = _currentUserService.UserId;
            var roles = _currentUserService.Role;

            if (string.IsNullOrEmpty(adminId) || roles == null || !roles.Contains("Admin"))
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "Bị cấm: Chỉ quản trị viên mới có thể thực hiện hành động này."
                };
            }

            // ✅ Convert DurationType to minutes
            int durationMinutes = request.DurationType?.ToLower() switch
            {
                "12 tiếng" => 12 * 60,
                "1 ngày" => 1 * 24 * 60,
                "3 ngày" => 3 * 24 * 60,
                "7 ngày" => 7 * 24 * 60,
                "15 ngày" => 15 * 24 * 60,
                "30 ngày" => 30 * 24 * 60,
                "vĩnh viễn" => 0,
                _ => -1
            };

            if (durationMinutes == -1 && request.isBanned)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "Loại thời hạn cấm không hợp lệ."
                };
            }

            // ✅ Tính thời điểm bannedUntil (ticks)
            long? bannedUntilTicks = null;
            if (request.isBanned && durationMinutes > 0)
            {
                var bannedUntil = TimeHelper.NowVN.AddMinutes(durationMinutes);
                bannedUntilTicks = bannedUntil.Ticks;
            }
            var results = new List<object>();
            // ✅ Cập nhật từng user
            foreach (var userId in request.UserIds)
            {
                var user = await _userRepository.GetById(userId);
                if (user != null)
                {
                    await _userRepository.UpdateLockvsUnLockUser(userId, request.isBanned, bannedUntilTicks);

                    // ✅ Gửi thông báo ban / unban
                    var notifyType = request.isBanned ? NotificationType.BanUser : NotificationType.UnBanUser;
                    var message = request.isBanned
                        ? (bannedUntilTicks.HasValue
                            ? $"Bạn đã bị khóa tài khoản đến {TimeHelper.FromTicks(bannedUntilTicks.Value):HH:mm dd/MM/yyyy} do vi phạm điều khoản."
                            : "Tài khoản của bạn đã bị khóa vĩnh viễn do vi phạm điều khoản.")
                        : "Tài khoản của bạn đã được mở khóa. Hãy tuân thủ quy định để tránh bị khóa lần nữa.";

                    var notifyResult = await _mediator.Send(new SendNotificationToUserCommand
                    {
                        SenderId = adminId,
                        UserId = userId,
                        Type = notifyType,
                        Message = message
                    });
                    bool signalRSent = false;

                    if (notifyResult.Success && notifyResult.Data is not null)
                    {
                        dynamic data = notifyResult.Data;
                        try
                        {
                            signalRSent = data.SignalRSent;
                        }
                        catch
                        {
                            signalRSent = false;
                        }
                    }


                    results.Add(new
                    {
                        UserId = userId,
                        SignalRSent = signalRSent,
                        NotificationMessage = message
                    });
                }
            }


            return new ApiResponse
            {
                Success = true,
                Message = request.isBanned
                ? $"Khóa {request.UserIds.Count} người dùng {(durationMinutes > 0 ? $"đến {durationMinutes / 60} giờ" : "vĩnh viễn")}."
                : $"Mở khóa {request.UserIds.Count} người dùng.",
                Data = results
            };
        }
    }
}
