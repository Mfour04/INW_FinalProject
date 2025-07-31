using AutoMapper;
using Domain.Entities.System;
using Infrastructure.Repositories.Implements;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Novel;
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
        public GetAllUserHanlder(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
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

            if (users == null || users.Count == 0)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "No users found."
                };
            }
            var userResponse = _mapper.Map<List<UserResponse>>(users);
            int totalPages = (int)Math.Ceiling((double)totalCount / request.Limit);

            return new ApiResponse
            {
                Success = true,
                Message = "Retrieved novels successfully.",
                Data = new
                {
                    Users = userResponse,
                    TotalUsers = totalCount,
                    TotalPages = totalPages
                }
            };
        }
    }
}
