using AutoMapper;
using Domain.Entities;
using Domain.Entities.System;
using Domain.Enums;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Tag;
using Shared.Contracts.Response.User;
using Shared.Helpers;

namespace Application.Features.User.Queries
{
    public class GetAllUser: IRequest<ApiResponse>
    {
        public string SortBy { get; set; } = "";
        public int Page { get; set; } = 0;
        public int Limit { get; set; } = int.MaxValue;
        public string? SearchTerm { get; set; } = "";
    }

    public class GetAllUserHanlder : IRequestHandler<GetAllUser, ApiResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly ITagRepository _tagRepository;
        public GetAllUserHanlder(IUserRepository userRepository, IMapper mapper
            , ITagRepository tagRepository)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _tagRepository = tagRepository;
        }

        public async Task<ApiResponse> Handle(GetAllUser request, CancellationToken cancellationToken)
        {
            var result = SystemHelper.ParseSearchQuerySmart(request.SearchTerm);
            var exact = result.Exact;
            var fuzzyTerms = result.FuzzyTerms;
            var sortBy = SystemHelper.ParseSortCriteria(request.SortBy);

            var findCriteria = new FindCreterias
            {
                Page = request.Page,
                Limit = request.Limit,
                SearchTerm = string.IsNullOrWhiteSpace(exact) ? new() : new List<string> { exact },
            };

            var (users, totalCount) = await _userRepository.GetAllUserAsync(findCriteria, sortBy);

            // Fallback: nếu không có, thử fuzzy
            if ((users == null || users.Count == 0) && fuzzyTerms.Any())
            {
                findCriteria.SearchTerm = fuzzyTerms;
                (users, totalCount) = await _userRepository.GetAllUserAsync(findCriteria, sortBy);
            }
            users = users?.Where(u => u.role != Role.Admin).ToList() ?? new List<UserEntity>();
            totalCount = users.Count;
            if (users == null || users.Count == 0)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "No users found."
                };
            }
            var allTagIds = users
            .Where(u => u.favourite_type != null)
            .SelectMany(u => u.favourite_type)
            .Distinct()
            .ToList();

            // Bước 2: Lấy thông tin tag từ repo
            var tagEntities = await _tagRepository.GetTagsByIdsAsync(allTagIds);
            var tagDict = tagEntities.ToDictionary(t => t.id, t => t.name);

            var userResponseList = _mapper.Map<List<UserResponse>>(users);

            foreach (var userResponse in userResponseList)
            {
                var originalUser = users.FirstOrDefault(u => u.id == userResponse.UserId);
                if (originalUser == null) continue;

                userResponse.FavouriteType = (originalUser.favourite_type ?? new List<string>())
                    .Where(tagId => tagDict.ContainsKey(tagId))
                    .Select(tagId => new TagListResponse
                    {
                        TagId = tagId,
                        Name = tagDict[tagId]
                    }).ToList();
            }


            int totalPages = (int)Math.Ceiling((double)totalCount / request.Limit);

            return new ApiResponse
            {
                Success = true,
                Message = "Retrieved novels successfully.",
                Data = new
                {
                    Users = userResponseList,
                    TotalUsers = totalCount,
                    TotalPages = totalPages
                }
            };
        }
    }
}
