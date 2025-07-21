using AutoMapper;
using Domain.Enums;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Transaction;

namespace Application.Features.Transaction.Queries
{
    public class GetTransactionById : IRequest<ApiResponse>
    {
        public string TransactionId { get; set; }
        public string CurrentUserId { get; set; }
        public bool IsAdmin { get; set; }
    }

    public class GetTransactionByIdHandler : IRequestHandler<GetTransactionById, ApiResponse>
    {
        private readonly ITransactionRepository _transactionRepo;
        private readonly ITransactionLogRepository _logRepo;
        private readonly IMapper _mapper;

        public GetTransactionByIdHandler(
            ITransactionRepository transactionRepo,
            ITransactionLogRepository logRepo,
            IMapper mapper)
        {
            _transactionRepo = transactionRepo;
            _logRepo = logRepo;
            _mapper = mapper;
        }

        public async Task<ApiResponse> Handle(GetTransactionById request, CancellationToken cancellationToken)
        {
            var transaction = await _transactionRepo.GetByIdAsync(request.TransactionId);
            if (transaction == null)
                return Fail("Transaction not found.");

            if (!request.IsAdmin && transaction.requester_id != request.CurrentUserId)
                return Fail("Unauthorized access to transaction.");

            object? mapped = null;

            switch (transaction.type)
            {
                case PaymentType.TopUp:
                    mapped = request.IsAdmin
                        ? _mapper.Map<AdminTopUpTransactionResponse>(transaction)
                        : _mapper.Map<TopUpTransactionResponse>(transaction);
                    break;

                case PaymentType.WithdrawCoin:
                    if (request.IsAdmin)
                    {
                        var withdraw = _mapper.Map<AdminWithdrawTransactionResponse>(transaction);
                        var log = await _logRepo.GetLogByTransactionIdAsync(transaction.id);
                        if (log != null)
                        {
                            withdraw.Message = log.message;
                            withdraw.ActionById = log.action_by_id;
                        }
                        mapped = withdraw;
                    }
                    else
                    {
                        var withdraw = _mapper.Map<WithdrawTransactionResponse>(transaction);
                        var log = await _logRepo.GetLogByTransactionIdAsync(transaction.id);
                        if (log != null)
                            withdraw.Message = log.message;

                        mapped = withdraw;
                    }
                    break;

                case PaymentType.BuyNovel:
                    mapped = request.IsAdmin
                        ? _mapper.Map<AdminBuyNovelTransactionResponse>(transaction)
                        : _mapper.Map<BuyNovelTransactionResponse>(transaction);
                    break;

                case PaymentType.BuyChapter:
                    mapped = request.IsAdmin
                        ? _mapper.Map<AdminBuyChapterTransactionResponse>(transaction)
                        : _mapper.Map<BuyChapterTransactionResponse>(transaction);
                    break;

                default:
                    return Fail("Unsupported transaction type.");
            }

            return new ApiResponse
            {
                Success = true,
                Message = "Transaction detail retrieved successfully.",
                Data = mapped
            };
        }

        private ApiResponse Fail(string msg) => new()
        {
            Success = false,
            Message = msg
        };
    }
}
