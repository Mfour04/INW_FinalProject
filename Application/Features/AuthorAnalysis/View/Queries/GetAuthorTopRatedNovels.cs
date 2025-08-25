using Application.Services.Interfaces;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.AuthorAnalysis;


namespace Application.Features.AuthorAnalysis.View.Queries
{
    public class GetAuthorTopRatedNovels : IRequest<ApiResponse>
    {
        public int Limit { get; set; } = 10;
    }
    public class GetAuthorTopRatedNovelsHandler : IRequestHandler<GetAuthorTopRatedNovels, ApiResponse>
    {
        private readonly INovelRepository _novelRepo;
        private readonly ICurrentUserService _current;
        public GetAuthorTopRatedNovelsHandler(INovelRepository novelRepo, ICurrentUserService current)
        {
            _novelRepo = novelRepo;
            _current = current;
        }
        public async Task<ApiResponse> Handle(GetAuthorTopRatedNovels request, CancellationToken ct)
        {
            var novels = await _novelRepo.GetNovelByAuthorId(_current.UserId!);

            var topNovels = novels
                .Where(n => n.rating_avg > 0) // Ensure the novel has ratings
                .OrderByDescending(n => n.rating_avg)
                .ThenByDescending(n => n.rating_count)
                .Take(request.Limit)
                .Select(n => new AuthorTopRatedNovelResponse
                {
                    NovelId = n.id,
                    Title = n.title,
                    RatingAvg = n.rating_avg,
                    RatingCount = n.rating_count
                })
            .ToList();

            var totalNovelCount = novels.Count;
            return new ApiResponse
            {
                Success = true,
                Message = "Retrieved top rated novels successfully.",
                Data = new
                {
                    TotalNovels = totalNovelCount,
                    TopRatedNovels = topNovels
                }
            };
        }
    }   
}
