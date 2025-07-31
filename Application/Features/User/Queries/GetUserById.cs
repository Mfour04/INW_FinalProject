using Application.Services.Interfaces;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Tag;
using Shared.Contracts.Response.User;

namespace Application.Features.User.Queries
{
    public class GetUserById : IRequest<ApiResponse>
    {
        public string UserId { get; set; }
        public string CurrentUserId { get; set; }
    }

    public class GetUserByIdHanlder : IRequestHandler<GetUserById, ApiResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;
        private readonly ITagRepository _tagRepository;
        public GetUserByIdHanlder(IUserRepository userRepository, IMapper mapper
            , ICurrentUserService currentUserService, ITagRepository tagRepository)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _currentUserService = currentUserService;
            _tagRepository = tagRepository;
        }
        public async Task<ApiResponse> Handle(GetUserById request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _userRepository.GetById(request.UserId);
                if (user == null)
                {
                    return new ApiResponse
                    {
                        Success = false,
                        Message = "User not found"
                    };
                }

                List<TagEntity> tagEntities = new();
                if (user.favourite_type != null && user.favourite_type.Any())
                {
                    tagEntities = await _tagRepository.GetTagsByIdsAsync(user.favourite_type);
                }

                var response = _mapper.Map<UserResponse>(user);

                response.FavouriteType = tagEntities.Select(tag => new TagListResponse
                {
                    TagId = tag.id,
                    Name = tag.name
                }).ToList();

                // Nếu không phải chính mình thì ẩn các trường nhạy cảm
                if (request.CurrentUserId != request.UserId)
                {
                    response.Email = null;
                    response.Coin = 0;
                    response.BlockCoin = 0;
                    // Có thể ẩn thêm nếu cần
                }

                return new ApiResponse
                {
                    Success = true,
                    Data = response
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse { Success = false, Message = $"An error occurred: {ex.Message}" };
            }
        }
    }
}
