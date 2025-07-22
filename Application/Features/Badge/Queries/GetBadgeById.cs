using AutoMapper;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Badge;

namespace Application.Features.Badge.Queries
{
    public class GetBadgeById : IRequest<ApiResponse>
    {
        public string Id { get; set; }
    }

    public class GetBadgeByIdHanlder : IRequestHandler<GetBadgeById, ApiResponse>
    {
        private readonly IBadgeRepository _badgeRepo;
        private readonly IMapper _mapper;

        public GetBadgeByIdHanlder(IBadgeRepository badgeRepo, IMapper mapper)
        {
            _badgeRepo = badgeRepo;
            _mapper = mapper;
        }

        public async Task<ApiResponse> Handle(GetBadgeById request, CancellationToken cancellationToken)
        {

            var badge = await _badgeRepo.GetByIdAsync(request.Id);
            if (badge == null)
            {
                return new ApiResponse { Success = false, Message = "No forum posts found." };
            }

            var response = _mapper.Map<BadgeResponse>(badge);

            return new ApiResponse { Success = true, Data = response };
        }
    }
}