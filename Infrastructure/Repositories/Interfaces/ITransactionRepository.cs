using Domain.Entities;
using Domain.Entities.System;
using Domain.Enums;

namespace Infrastructure.Repositories.Interfaces
{
    public interface ITransactionRepository
    {
        Task<List<TransactionEntity>> GetAllAsync(PaymentType? type, FindCreterias creterias, List<SortCreterias> sortCreterias);
        Task AddAsync(TransactionEntity transaction);
        Task<bool> UpdateStatusAsync(string id, TransactionEntity entity);
        Task<TransactionEntity> GetByOrderCodeAsync(long orderCode);
        Task<List<TransactionEntity>> GetExpiredPendingTransactionsAsync(long timeoutTimestamp);
        Task<(List<TransactionEntity> Transactions, int TotalCount)> GetUserTransactionsAsync(string userId, PaymentType? type, FindCreterias creterias, List<SortCreterias> sortCreterias);
        Task<TransactionEntity> GetByIdAsync(string id);
        Task<List<TransactionEntity>> GetTransactionsByIdsAsync(List<string> transactionIds);
        Task<List<TransactionEntity>> GetTransactionsByNovelIdsAsync(List<string> novelIds, long startTicks, long endTicks, int[] types);
        Task<List<TransactionEntity>> GetPendingWithdrawRequestsAsync(FindCreterias creterias, List<SortCreterias> sortCreterias);
        Task<List<TransactionEntity>> GetCompletedTransactionsInRangeAsync(long startDate, long endDate);
    }
}