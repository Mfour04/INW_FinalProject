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
        private readonly IMapper _mapper;

        public GetUserTransactionHandler(ITransactionRepository transactionRepo, IMapper mapper)
        {
            _transactionRepo = transactionRepo;
            _mapper = mapper;
        }

        public async Task<ApiResponse> Handle(GetUserTransaction request, CancellationToken cancellationToken)
        {
            FindCreterias findCreterias = new();
            findCreterias.Limit = request.Limit;
            findCreterias.Page = request.Page;

            var sortBy = SystemHelper.ParseSortCriteria(request.SortBy);

            var userTransaction = await _transactionRepo.GetUserTransactionsAsync(request.UserId, request.Type, findCreterias, sortBy);
            if (userTransaction == null || userTransaction.Count == 0)
                return new ApiResponse { Success = false, Message = "No transaction found." };

            var response = _mapper.Map<List<UserTransactionResponse>>(userTransaction);
            return new ApiResponse
            {
                Success = true,
                Message = "Retrieved user's transaction successfully.",
                Data = response
            };
        }
    }
}