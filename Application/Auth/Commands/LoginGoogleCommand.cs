using AutoMapper;
using Domain.Entities;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.User;
using Shared.Helpers;
using Shared.SystemHelpers.TokenGenerate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Auth.Commands
{
    public class LoginGoogleCommand : IRequest<ApiResponse>
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public string AvatarUrl { get; set; }
    }

    public class LoginGoogleCommandHandler : IRequestHandler<LoginGoogleCommand, ApiResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly JwtHelpers _jwtHelpers;
        private readonly IMapper _mapper;
        public LoginGoogleCommandHandler(IUserRepository userRepository, JwtHelpers jwtHelpers, IMapper mapper)
        {
            _userRepository = userRepository;
            _jwtHelpers = jwtHelpers;
            _mapper = mapper;
        }
        public async Task<ApiResponse> Handle(LoginGoogleCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.FindOrCreateUserFromGoogleAsync(
                 request.Email,
                 request.Name,
                 request.AvatarUrl
             );

            var userResponse = _mapper.Map<UserResponse>(user);
            // Generate tokens
            var accessToken = _jwtHelpers.Generate(user.id, user.displayname, user.role.ToString());
            var refreshToken = _jwtHelpers.GenerateRefreshToken(user.id);

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
