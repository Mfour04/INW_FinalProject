using Application.Services.Interfaces;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.AuthorAnalysis;

namespace Application.Features.AuthorAnalysis.View.Queries
{
    public class GetAuthorTopViewedNovels : IRequest<ApiResponse>
    {
        public int Limit { get; set; } = 10;
    }

    public class GetAuthorTopViewedNovelsHandler : IRequestHandler<GetAuthorTopViewedNovels, ApiResponse>
    {
        private readonly INovelRepository _novelRepo;
        private readonly ICurrentUserService _current;

        public GetAuthorTopViewedNovelsHandler(INovelRepository novelRepo, ICurrentUserService current)
        {
            _novelRepo = novelRepo;
            _current = current;
        }

        public async Task<ApiResponse> Handle(GetAuthorTopViewedNovels request, CancellationToken ct)
        {
            var novels = await _novelRepo.GetNovelByAuthorId(_current.UserId!);

            var topNovels = novels
                .OrderByDescending(n => n.total_views)
                .Take(request.Limit)
                .Select(n => new AuthorTopViewedNovelResponse
                {
                    NovelId = n.id,
                    Title = n.title,
                    TotalViews = n.total_views
                })
                .ToList();

            return new ApiResponse
            {
                Success = true,
                Message = "Retrieved top viewed novels successfully.",
                Data = topNovels
            };
        }
    }
}