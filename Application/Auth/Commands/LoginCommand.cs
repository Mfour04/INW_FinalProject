using Application.Exceptions;
using Application.Features.Notification.Commands;
using Application.Services.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Repositories.Implements;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Tag;
using Shared.Contracts.Response.User;
using Shared.Helpers;
using Shared.SystemHelpers.TokenGenerate;

namespace Application.Auth.Commands
{
    public class LoginCommand : IRequest<ApiResponse>
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }

    public class LoginHandler : IRequestHandler<LoginCommand, ApiResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly JwtHelpers _jwtHelpers;
        private readonly IMapper _mapper;
        private readonly INotificationRepository _notificationRepository;
        private readonly INotificationService _notificationService;
        private readonly ITagRepository _tagRepository;
        public LoginHandler(IUserRepository userRepository, JwtHelpers jwtHelpers, IMapper mapper
            , INotificationRepository notificationRepository, INotificationService notificationService
            , ITagRepository tagRepository)
        {
            _userRepository = userRepository;
            _jwtHelpers = jwtHelpers;
            _mapper = mapper;
            _notificationRepository = notificationRepository;
            _notificationService = notificationService;
            _tagRepository = tagRepository;
        }
        
        public async Task<ApiResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.UserName) || string.IsNullOrWhiteSpace(request.Password))
            {
                throw new ApiException("Email and Password are required.");
            }

            var user = await _userRepository.GetByName(request.UserName);
            if (user == null)
            {
                throw new ApiException("Invalid userName or password.");
            }

            if (user.is_banned)
            {
                if (TimeHelper.IsBanExpired(user.banned_until))
                {
                    // Ban đã hết hạn => tự động gỡ ban
                    user.is_banned = false;
                    user.banned_until = null;
                    await _userRepository.UpdateLockvsUnLockUser(user.id, false, null);
                    await _notificationRepository.CreateAsync(new NotificationEntity
                    {
                        id = SystemHelper.RandomId(),
                        user_id = user.id,
                        type = NotificationType.UnBanUser,
                        message = "Tài khoản của bạn đã được mở khóa tự động sau thời gian khóa.",
                        is_read = false,
                        created_at = TimeHelper.NowVN.Ticks
                    });

                    await _notificationService.SendNotificationAsync(user.id, "Tài khoản của bạn đã được mở khóa tự động sau thời gian khóa.", NotificationType.UnBanUser);
                }
                else
                {
                    // Ban chưa hết hạn => chặn đăng nhập
                    var bannedUntilText = TimeHelper.FromTicks(user.banned_until.Value).ToString("HH:mm dd/MM/yyyy");
                    throw new ApiException($"This account will be banned until {bannedUntilText}.");
                }
            }

            // Kiểm tra mật khẩu
            var isValidPassword = BCrypt.Net.BCrypt.Verify(request.Password, user.password);
            if (!isValidPassword)
            {
                throw new ApiException("Invalid email or password.");
            }

            var accessToken = _jwtHelpers.Generate(user.id, user.username, user.role.ToString());
            var refreshToken = _jwtHelpers.GenerateRefreshToken(user.id);

            List<TagEntity> tagEntities = new();
            if (user.favourite_type != null && user.favourite_type.Any())
            {
                tagEntities = await _tagRepository.GetTagsByIdsAsync(user.favourite_type);
            }
            var userResponse = _mapper.Map<UserResponse>(user);
            userResponse.FavouriteType = tagEntities.Select(tag => new TagListResponse
            {
                TagId = tag.id,
                Name = tag.name
            }).ToList();
            userResponse.LastLogin = TimeHelper.NowVN.Ticks;
            return new ApiResponse
            {
                Success = true,
                Message = "Login successful.",
                Data = new TokenResult
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    User = userResponse
                }
            };
        }
    }
}
