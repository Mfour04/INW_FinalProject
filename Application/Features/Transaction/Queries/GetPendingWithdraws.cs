using AutoMapper;
using Domain.Entities.System;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Transaction;
using Shared.Helpers;

namespace Application.Features.Transaction.Queries
{
    public class GetPendingWithdraws : IRequest<ApiResponse>
    {
        public int Page { get; set; } = 0;
        public int Limit { get; set; } = int.MaxValue;
        public string SortBy { get; set; } = "created_at:desc";
    }

    public class GetPendingWithdrawsHandler : IRequestHandler<GetPendingWithdraws, ApiResponse>
    {
        private readonly ITransactionRepository _transactionRepo;
        private readonly IUserBankAccountRepository _userBankAccountRepo;
        private readonly IMapper _mapper;

        public GetPendingWithdrawsHandler(ITransactionRepository transactionRepo, IUserBankAccountRepository userBankAccountRepo, IMapper mapper)
        {
            _transactionRepo = transactionRepo;
            _userBankAccountRepo = userBankAccountRepo;
            _mapper = mapper;
        }

        public async Task<ApiResponse> Handle(GetPendingWithdraws request, CancellationToken cancellationToken)
        {
            FindCreterias findCreterias = new()
            {
                Limit = request.Limit,
                Page = request.Page
            };

            var sortBy = SystemHelper.ParseSortCriteria(request.SortBy);

            var transactions = await _transactionRepo.GetPendingWithdrawRequestsAsync(findCreterias, sortBy);
            if (transactions == null || transactions.Count == 0)
                return new ApiResponse { Success = false, Message = "No request found." };

            var bankDict = (await _userBankAccountRepo
                .GetByIdsAsync(transactions
                    .Where(x => x.bank_account_id != null)
                    .Select(x => x.bank_account_id)
                    .Distinct()
                    .ToList()))
                .ToDictionary(b => b.id, b => b);

            var response = transactions.Select(tx =>
            {
                var mapped = _mapper.Map<AdminWithdrawTransactionResponse>(tx);

                if (tx.bank_account_id != null && bankDict.TryGetValue(tx.bank_account_id, out var bank))
                {
                    mapped.BankInfo = new AdminWithdrawTransactionResponse.UserBankInfomation
                    {
                        BankBin = bank.bank_bin,
                        BankAccountNumber = bank.bank_account_number,
                        BankAccountName = bank.bank_account_name
                    };
                }

                return mapped;
            }).ToList();

            return new ApiResponse
            {
                Success = true,
                Message = "Retrieved pending withdraw requests successfully.",
                Data = response
            };
        }
    }
}