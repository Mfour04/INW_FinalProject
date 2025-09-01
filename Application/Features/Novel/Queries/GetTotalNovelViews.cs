using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;

namespace Application.Features.Novel.Queries
{
    public class GetTotalNovelViews : IRequest<ApiResponse> { }

    public class GetTotalNovelViewsHandler : IRequestHandler<GetTotalNovelViews, ApiResponse>
    {
        private readonly INovelRepository _novelRepository;

        public GetTotalNovelViewsHandler(INovelRepository novelRepository)
        {
            _novelRepository = novelRepository;
        }

        public async Task<ApiResponse> Handle(GetTotalNovelViews request, CancellationToken cancellationToken)
        {
            var totalViews = await _novelRepository.GetTotalViewsAsync();

            return new ApiResponse
            {
                Success = true,
                Data = totalViews
            };
        }
    }
}