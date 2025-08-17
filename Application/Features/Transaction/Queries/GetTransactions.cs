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
    public class GetTransactions : IRequest<ApiResponse>
    {
        public PaymentType? Type { get; set; }
        public int Page { get; set; } = 0;
        public int Limit { get; set; } = int.MaxValue;
        public string SortBy { get; set; } = "created_at:desc";
    }

    public class GetTransactionsHandler : IRequestHandler<GetTransactions, ApiResponse>
    {
        private readonly ITransactionRepository _transactionRepo;
        private readonly ITransactionLogRepository _logRepo;
        private readonly IUserBankAccountRepository _userBankAccountRepo;
        private readonly IMapper _mapper;

        public GetTransactionsHandler(
            ITransactionRepository transactionRepo,
            ITransactionLogRepository logRepo,
            IUserBankAccountRepository userBankAccountRepo, 
            IMapper mapper)
        {
            _transactionRepo = transactionRepo;
            _logRepo = logRepo;
            _userBankAccountRepo = userBankAccountRepo;
            _mapper = mapper;
        }

        public async Task<ApiResponse> Handle(GetTransactions request, CancellationToken cancellationToken)
        {
            FindCreterias findCreterias = new()
            {
                Limit = request.Limit,
                Page = request.Page
            };

            var sortBy = SystemHelper.ParseSortCriteria(request.SortBy);

            var transactions = await _transactionRepo.GetAllAsync(request.Type, findCreterias, sortBy);
            if (transactions == null || transactions.Count == 0)
                return new ApiResponse { Success = false, Message = "No transaction found." };

            var responseList = new List<object>();
            var withdrawIds = new List<string>();
            var bankAccountIds = new HashSet<string>();
            
             foreach (var tx in transactions)
            {
                switch (tx.type)
                {
                    case PaymentType.TopUp:
                        responseList.Add(_mapper.Map<AdminTopUpTransactionResponse>(tx));
                        break;

                    case PaymentType.WithdrawCoin:
                    {
                        var withdraw = _mapper.Map<AdminWithdrawTransactionResponse>(tx);
                        withdrawIds.Add(tx.id);

                        if (!string.IsNullOrEmpty(tx.bank_account_id))
                            bankAccountIds.Add(tx.bank_account_id);

                        responseList.Add(withdraw);
                        break;
                    }

                    case PaymentType.BuyNovel:
                        responseList.Add(_mapper.Map<AdminBuyNovelTransactionResponse>(tx));
                        break;

                    case PaymentType.BuyChapter:
                        responseList.Add(_mapper.Map<AdminBuyChapterTransactionResponse>(tx));
                        break;
                }
            }

            if (withdrawIds.Count > 0)
            {
                var logs = await _logRepo.GetLogsByTransactionIdsAsync(withdrawIds);
                var logDict = logs.ToDictionary(
                    x => x.transaction_id,
                    x => new { x.message, x.action_by_id, x.action_type });

                foreach (var item in responseList)
                {
                    if (item is AdminWithdrawTransactionResponse withdraw &&
                        logDict.TryGetValue(withdraw.Id, out var log))
                    {
                        withdraw.Message = log.message;
                        withdraw.ActionById = log.action_by_id;
                        withdraw.ActionType = log.action_type;
                    }
                }
            }

            if (bankAccountIds.Count > 0)
            {
                var bankAccounts = await _userBankAccountRepo.GetByIdsAsync(bankAccountIds.ToList());
                var bankDict = bankAccounts.ToDictionary(b => b.id, b => b);

                for (int i = 0; i < transactions.Count && i < responseList.Count; i++)
                {
                    var tx = transactions[i];
                    if (responseList[i] is AdminWithdrawTransactionResponse withdraw &&
                        !string.IsNullOrEmpty(tx.bank_account_id) &&
                        bankDict.TryGetValue(tx.bank_account_id, out var bank))
                    {
                        withdraw.BankInfo = new AdminWithdrawTransactionResponse.UserBankInfomation
                        {
                            BankBin = bank.bank_bin,
                            BankAccountNumber = bank.bank_account_number,
                            BankAccountName = bank.bank_account_name
                        };
                    }
                }
            }

            return new ApiResponse
            {
                Success = true,
                Message = "Retrieved admin transaction list successfully.",
                Data = responseList
            };
        }
    }
}