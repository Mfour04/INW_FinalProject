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
                return new ApiResponse
                {
                    Success = true,
                    Message = "Sai tài khoản hoặc mật khẩu",
                    Data = null
                };
            }

            var user = await _userRepository.GetByName(request.UserName);
            if (user == null)
            {
                return new ApiResponse
                {
                    Success = true,
                    Message = "Sai tài khoản hoặc mật khẩu",
                    Data = null
                };
            }

            // Kiểm tra mật khẩu
            var isValidPassword = BCrypt.Net.BCrypt.Verify(request.Password, user.password);
            if (!isValidPassword)
            {
                return new ApiResponse
                {
                    Success = true,
                    Message = "Sai tài khoản hoặc mật khẩu",
                    Data = null
                };
            }

            // Nếu user bị ban mà hết hạn thì gỡ ban
            if (user.is_banned && TimeHelper.IsBanExpired(user.banned_until))
            {
                user.is_banned = false;
                user.banned_until = null;
                await _userRepository.UpdateLockvsUnLockUser(user.id, false, null);
                await _notificationService.SendNotificationToUsersAsync(
                    new List<string> { user.id },
                    "Your account has been automatically unlocked after the lock period.",
                    NotificationType.UnBanUser
                );
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
                Message = "Đăng nhập thành công",
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
