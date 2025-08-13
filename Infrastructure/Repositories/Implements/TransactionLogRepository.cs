using Domain.Entities;
using Infrastructure.InwContext;
using Infrastructure.Repositories.Interfaces;
using MongoDB.Driver;
using Shared.Exceptions;

namespace Infrastructure.Repositories.Implements
{
    public class TransactionLogRepository : ITransactionLogRepository
    {
        private readonly IMongoCollection<TransactionLogEntity> _collection;

        public TransactionLogRepository(MongoDBHelper mongoDBHelper)
        {
            mongoDBHelper.CreateCollectionIfNotExistsAsync("transaction_log").Wait();
            _collection = mongoDBHelper.GetCollection<TransactionLogEntity>("transaction_log");
        }

        /// <summary>
        /// Thêm log giao dịch
        /// </summary>
        public async Task AddAsync(TransactionLogEntity entity)
        {
            try
            {
                await _collection.InsertOneAsync(entity);
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        /// <summary>
        /// Lấy thông tin log của tất cả giao dịch 
        /// </summary>
        public async Task<List<TransactionLogEntity>> GetLogsByTransactionIdsAsync(List<string> transactionIds)
        {
            try
            {
                var filter = Builders<TransactionLogEntity>.Filter.In(x => x.transaction_id, transactionIds);
                var result = await _collection.Find(filter).ToListAsync();
                return result;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        /// <summary>
        /// Lấy thông tin log của giao dịch 
        /// </summary>
        public async Task<TransactionLogEntity> GetLogByTransactionIdAsync(string transactionId)
        {
            try
            {
                var result = await _collection.Find(x => x.transaction_id == transactionId).FirstOrDefaultAsync();
                return result;
            }
            catch
            {
                throw new InternalServerException();
            }
        }
    }
}