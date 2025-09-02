using Application.Services.Interfaces;
using Domain.Enums;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.AuthorAnaysis;

namespace Application.Features.AuthorAnalysis.Earning.Queries
{
    public class GetAuthorTopNovels : IRequest<ApiResponse>
    {
        public int Limit { get; set; } = 5;
    }

    public class GetAuthorTopNovelsHandler : IRequestHandler<GetAuthorTopNovels, ApiResponse>
    {
        private readonly IAuthorEarningRepository _authorEarningRepo;
        private readonly INovelRepository _novelRepo;
        private readonly ICurrentUserService _currentUser;

        public GetAuthorTopNovelsHandler(
            IAuthorEarningRepository authorEarningRepo,
            INovelRepository novelRepo,
            ICurrentUserService currentUser)
        {
            _authorEarningRepo = authorEarningRepo;
            _novelRepo = novelRepo;
            _currentUser = currentUser;
        }

        public async Task<ApiResponse> Handle(GetAuthorTopNovels request, CancellationToken cancellationToken)
        {
            var raws = await _authorEarningRepo.GetByAuthorIdAsync(
                _currentUser.UserId!, 0, long.MaxValue);

            var grouped = raws
                .GroupBy(x => x.novel_id)
                .Select(g => new
                {
                    NovelId = g.Key,
                    TotalCoins = g.Sum(x => x.amount),
                    TotalOrders = g.Count()
                })
                .OrderByDescending(x => x.TotalCoins)
                .Take(request.Limit)
                .ToList();

            var novelIds = grouped.Select(x => x.NovelId).ToList();
            var novels = await _novelRepo.GetManyByIdsAsync(novelIds);
            var novelMap = novels.ToDictionary(n => n.id, n => n);

            var result = grouped.Select(x => new AuthorTopNovelResponse
            {
                NovelId = x.NovelId,
                Title = novelMap.TryGetValue(x.NovelId, out var nv) ? nv.title : "",
                TotalCoins = x.TotalCoins,
                TotalOrders = x.TotalOrders
            }).ToList();

            return new ApiResponse
            {
                Success = true,
                Message = "Retrieved top novels successfully.",
                Data = result
            };
        }
    }
}