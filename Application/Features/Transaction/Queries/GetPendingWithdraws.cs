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
        private readonly IMapper _mapper;

        public GetPendingWithdrawsHandler(ITransactionRepository transactionRepo, IMapper mapper)
        {
            _transactionRepo = transactionRepo;
            _mapper = mapper;
        }

        public async Task<ApiResponse> Handle(GetPendingWithdraws request, CancellationToken cancellationToken)
        {
            FindCreterias findCreterias = new();
            findCreterias.Limit = request.Limit;
            findCreterias.Page = request.Page;

            var sortBy = SystemHelper.ParseSortCriteria(request.SortBy);

            var transactions = await _transactionRepo.GetPendingWithdrawRequestsAsync(findCreterias, sortBy);
            if (transactions == null || transactions.Count == 0)
                return new ApiResponse { Success = false, Message = "No request found." };

            var response = _mapper.Map<List<TransactionResponse>>(transactions);
            return new ApiResponse
            {
                Success = true,
                Message = "Retrieved pending withdraw requests successfully.",
                Data = response
            };
        }
    }
}