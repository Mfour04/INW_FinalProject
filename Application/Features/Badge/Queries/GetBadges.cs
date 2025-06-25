using AutoMapper;
using Domain.Entities.System;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Badge;
using Shared.Helpers;

namespace Application.Features.Badge.Queries
{
    public class GetBadges : IRequest<ApiResponse>
    {
        public string SortBy { get; set; } = "created_at:desc";
        public int Page { get; set; } = 0;
        public int Limit { get; set; } = int.MaxValue;
    }

    public class GetBadgesHanlder : IRequestHandler<GetBadges, ApiResponse>
    {
        private readonly IBadgeRepository _badgeRepo;
        private readonly IMapper _mapper;

        public GetBadgesHanlder(IBadgeRepository badgeRepo, IMapper mapper)
        {
            _badgeRepo = badgeRepo;
            _mapper = mapper;
        }

        public async Task<ApiResponse> Handle(GetBadges request, CancellationToken cancellationToken)
        {
            FindCreterias findCreterias = new();
            findCreterias.Limit = request.Limit;
            findCreterias.Page = request.Page;

            var sortBy = SystemHelper.ParseSortCriteria(request.SortBy);

            var badges = await _badgeRepo.GetAllAsync(findCreterias, sortBy);

            if (badges == null || badges.Count == 0)
                return new ApiResponse { Success = false, Message = "No badges found." };

            var response = _mapper.Map<List<BadgeResponse>>(badges);

            return new ApiResponse
            {
                Success = true,
                Message = "Retrieved badges successfully.",
                Data = response
            };
        }
    }
}