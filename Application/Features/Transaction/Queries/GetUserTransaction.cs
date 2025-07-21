using AutoMapper;
using Domain.Entities.System;
using Domain.Enums;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Transaction;
using Shared.Helpers;

namespace Application.Features.Transaction.Queries
{
    public class GetUserTransaction : IRequest<ApiResponse>
    {
        public string? UserId { get; set; }
        public PaymentType? Type { get; set; }
        public int Page { get; set; } = 0;
        public int Limit { get; set; } = int.MaxValue;
        public string SortBy { get; set; } = "created_at:desc";
    }

    public class GetUserTransactionHandler : IRequestHandler<GetUserTransaction, ApiResponse>
    {
        private readonly ITransactionRepository _transactionRepo;
        private readonly ITransactionLogRepository _logRepo;
        private readonly IMapper _mapper;

        public GetUserTransactionHandler(
           ITransactionRepository transactionRepo,
           ITransactionLogRepository logRepo,
           IMapper mapper)
        {
            _transactionRepo = transactionRepo;
            _logRepo = logRepo;
            _mapper = mapper;
        }

        public async Task<ApiResponse> Handle(GetUserTransaction request, CancellationToken cancellationToken)
        {
            FindCreterias findCreterias = new()
            {
                Limit = request.Limit,
                Page = request.Page
            };

            var sortBy = SystemHelper.ParseSortCriteria(request.SortBy);

            var userTransaction = await _transactionRepo.GetUserTransactionsAsync(request.UserId, request.Type, findCreterias, sortBy);
            if (userTransaction == null || userTransaction.Count == 0)
                return new ApiResponse { Success = false, Message = "No transaction found." };

            var responseList = new List<object>();
            var withdrawIds = new List<string>();

            foreach (var tx in userTransaction)
            {
                object mapped;

                switch (tx.type)
                {
                    case PaymentType.TopUp:
                        mapped = _mapper.Map<TopUpTransactionResponse>(tx);
                        break;

                    case PaymentType.WithdrawCoin:
                        var withdraw = _mapper.Map<WithdrawTransactionResponse>(tx);
                        withdrawIds.Add(tx.id);
                        mapped = withdraw;
                        break;

                    case PaymentType.BuyNovel:
                        mapped = _mapper.Map<BuyNovelTransactionResponse>(tx);
                        break;

                    case PaymentType.BuyChapter:
                        mapped = _mapper.Map<BuyChapterTransactionResponse>(tx);
                        break;

                    default:
                        continue;
                }

                responseList.Add(mapped);
            }

            if (withdrawIds.Count > 0)
            {
                var logs = await _logRepo.GetLogsByTransactionIdsAsync(withdrawIds);
                var logDict = logs.ToDictionary(x => x.transaction_id, x => x.message);

                foreach (var item in responseList)
                {
                    if (item is WithdrawTransactionResponse withdraw &&
                        logDict.TryGetValue(withdraw.Id, out var msg))
                    {
                        withdraw.Message = msg;
                    }
                }
            }

            return new ApiResponse
            {
                Success = true,
                Message = "Retrieved user's transaction successfully.",
                Data = responseList
            };
        }
    }
}
