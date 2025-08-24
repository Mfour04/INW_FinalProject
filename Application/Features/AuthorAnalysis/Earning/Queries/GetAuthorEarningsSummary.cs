using Application.Services.Interfaces;
using Domain.Enums;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.AuthorAnaysis;

namespace Application.Features.AuthorAnalysis.Earning.Queries
{
    public class GetAuthorEarningsSummary : IRequest<ApiResponse>
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Filter { get; set; } = "all";
        public string? NovelId { get; set; }

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

    }

    public class GetAuthorEarningsSummaryHandler : IRequestHandler<GetAuthorEarningsSummary, ApiResponse>
    {
        private readonly IAuthorEarningRepository _authorEarningRepo;
        private readonly ITransactionRepository _transactionRepo;
        private readonly IUserRepository _userRepo;
        private readonly ICurrentUserService _currentUser;

        public GetAuthorEarningsSummaryHandler(
            IAuthorEarningRepository authorEarningRepo,
            ITransactionRepository transactionRepo,
            IUserRepository userRepo,
            ICurrentUserService currentUser)
        {
            _authorEarningRepo = authorEarningRepo;
            _transactionRepo = transactionRepo;
            _userRepo = userRepo;
            _currentUser = currentUser;
        }

        public async Task<ApiResponse> Handle(GetAuthorEarningsSummary request, CancellationToken cancellationToken)
        {
            long startTicks = request.StartDate?.Date.Ticks ?? 0L;
            long endTicks = request.EndDate?.Date.AddDays(1).AddTicks(-1).Ticks ?? long.MaxValue;

            var raws = string.IsNullOrWhiteSpace(request.NovelId)
               ? await _authorEarningRepo.GetByAuthorIdAsync(_currentUser.UserId!, startTicks, endTicks)
               : await _authorEarningRepo.GetByNovelAsync(_currentUser.UserId!, request.NovelId!, startTicks, endTicks);

            var f = (request.Filter ?? "all").Trim().ToLowerInvariant();
            IEnumerable<Domain.Entities.AuthorEarningEntity> rows = f switch
            {
                "novel" => raws.Where(x => x.type == PaymentType.BuyNovel),
                "chapter" => raws.Where(x => x.type == PaymentType.BuyChapter),
                _ => raws
            };

            // tổng coin & đơn
            int totalCoins = rows.Sum(x => x.amount);
            int totalOrders = rows.Count();

            var txIds = rows
                .Select(x => x.source_transaction_id)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToList();

            var txList = txIds.Count == 0
                ? new List<Domain.Entities.TransactionEntity>()
                : await _transactionRepo.GetTransactionsByIdsAsync(txIds);

            var txMap = txList.ToDictionary(t => t.id, t => t);

            var buyerIds = txList
                .Select(t => t.requester_id)
                .Where(uid => !string.IsNullOrWhiteSpace(uid))
                .Distinct()
                .ToList();

            var buyers = buyerIds.Count == 0
                ? new List<Domain.Entities.UserEntity>()
                : await _userRepo.GetUsersByIdsAsync(buyerIds);

            var usernameMap = buyers.ToDictionary(u => u.id, u => u.username ?? "");
            var displayNameMap = buyers.ToDictionary(u => u.id, u => u.displayname ?? "");

            int page = request.Page <= 0 ? 1 : request.Page;
            int pageSize = request.PageSize <= 0 ? 10 : request.PageSize;

            int totalLogs = rows.Count();

            var pageRows = rows
                .OrderByDescending(x => x.created_at)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var logs = pageRows.Select(x =>
            {
                string? buyerUsername = null;
                string? buyerDisplayName = null;

                if (!string.IsNullOrWhiteSpace(x.source_transaction_id) &&
                    txMap.TryGetValue(x.source_transaction_id, out var tx) &&
                    !string.IsNullOrWhiteSpace(tx.requester_id))
                {
                    usernameMap.TryGetValue(tx.requester_id, out buyerUsername);
                    displayNameMap.TryGetValue(tx.requester_id, out buyerDisplayName);
                }

                return new AuthorEarningPurchaseLogItem
                {
                    EarningId = x.id,
                    NovelId = x.novel_id,
                    ChapterId = x.chapter_id,
                    Type = x.type.ToString(),
                    Amount = x.amount,
                    CreatedAt = x.created_at,
                    BuyerUsername = buyerUsername,
                    BuyerDisplayName = buyerDisplayName
                };
            }).ToList();

            var data = new AuthorEarningsSummaryResponse
            {
                TotalEarningsCoins = totalCoins,
                TotalOrders = totalOrders,
                FilterApplied = f,
                NovelId = request.NovelId,

                Logs = logs,
                TotalLogs = totalLogs,
                Page = page,
                PageSize = pageSize,
                HasMore = page * pageSize < totalLogs
            };

            return new ApiResponse
            {
                Success = true,
                Message = "Retrieved author earnings summary successfully.",
                Data = data
            };
        }
    }
}
