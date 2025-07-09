using Domain.Entities;
using Domain.Entities.System;
using Domain.Enums;
using Infrastructure.InwContext;
using Infrastructure.Repositories.Interfaces;
using MongoDB.Driver;
using Shared.Exceptions;

namespace Infrastructure.Repositories.Implements
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly IMongoCollection<TransactionEntity> _collection;

        public TransactionRepository(MongoDBHelper mongoDBHelper)
        {
            mongoDBHelper.CreateCollectionIfNotExistsAsync("transactions").Wait();
            _collection = mongoDBHelper.GetCollection<TransactionEntity>("transactions");
        }

        /// <summary>
        /// Lấy toàn bộ danh sách giao dịch, có thể lọc theo loại giao dịch.
        /// </summary>
        public async Task<List<TransactionEntity>> GetAllAsync(
            PaymentType? type,
            FindCreterias creterias,
            List<SortCreterias> sortCreterias)
        {
            try
            {
                var builder = Builders<TransactionEntity>.Filter;
                var filter = builder.Empty;

                if (type.HasValue)
                {
                    filter &= builder.Eq(t => t.type, type.Value);
                }

                var query = _collection.Find(filter);

                var sortBuilder = Builders<TransactionEntity>.Sort;
                var sortDefinitions = new List<SortDefinition<TransactionEntity>>();

                foreach (var criterion in sortCreterias)
                {
                    SortDefinition<TransactionEntity>? sortDef = criterion.Field switch
                    {
                        "completed_at" => criterion.IsDescending
                            ? sortBuilder.Descending(t => t.completed_at)
                            : sortBuilder.Ascending(t => t.completed_at),

                        "amount" => criterion.IsDescending
                            ? sortBuilder.Descending(t => t.amount)
                            : sortBuilder.Ascending(t => t.amount),

                        _ => null
                    };

                    if (sortDef != null)
                        sortDefinitions.Add(sortDef);
                }

                if (sortDefinitions.Any())
                {
                    query = query.Sort(sortBuilder.Combine(sortDefinitions));
                }

                query = query
                    .Skip(creterias.Page * creterias.Limit)
                    .Limit(creterias.Limit);

                return await query.ToListAsync();
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        /// <summary>
        /// Tạo mới một giao dịch.
        /// </summary>
        public async Task AddAsync(TransactionEntity transaction)
        {
            try
            {
                await _collection.InsertOneAsync(transaction);
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        /// <summary>
        /// Lấy giao dịch theo mã đơn hàng (orderCode) nạp PayOS.
        /// </summary>
        public async Task<TransactionEntity> GetByOrderCodeAsync(long orderCode)
        {
            try
            {
                return await _collection.Find(t => t.id == orderCode.ToString()).FirstOrDefaultAsync();
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        /// <summary>
        /// Cập nhật trạng thái (status) của giao dịch theo id.
        /// </summary>
        public async Task UpdateStatusAsync(string id, PaymentStatus newStatus)
        {
            try
            {
                var update = Builders<TransactionEntity>.Update.Set(t => t.status, newStatus);
                await _collection.UpdateOneAsync(t => t.id == id, update);
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        /// <summary>
        /// Lấy danh sách giao dịch nạp tiền có trạng thái Pending nhưng đã quá thời gian timeout.
        /// </summary>
        public async Task<List<TransactionEntity>> GetExpiredPendingTransactionsAsync(long timeoutTimestamp)
        {

            try
            {
                var builder = Builders<TransactionEntity>.Filter;

                var filter = builder.And(
                    builder.Eq(t => t.status, PaymentStatus.Pending),
                    builder.Eq(t => t.type, PaymentType.TopUp),
                    builder.Lt(t => t.created_at, timeoutTimestamp)
                );

                var result = await _collection.Find(filter).ToListAsync();
                return result;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        /// <summary>
        /// Lấy danh sách giao dịch của người dùng theo loại giao dịch.
        /// </summary>
        public async Task<List<TransactionEntity>> GetUserTransactionsAsync(
            string userId,
            PaymentType? type,
            FindCreterias creterias,
            List<SortCreterias> sortCreterias)
        {
            try
            {
                var builder = Builders<TransactionEntity>.Filter;
                var filter = builder.Eq(t => t.user_id, userId);

                if (type.HasValue)
                {
                    filter &= builder.Eq(t => t.type, type.Value);
                }

                var query = _collection.Find(filter);

                var sortBuilder = Builders<TransactionEntity>.Sort;
                var sortDefinitions = new List<SortDefinition<TransactionEntity>>();

                foreach (var criterion in sortCreterias)
                {
                    SortDefinition<TransactionEntity>? sortDef = criterion.Field switch
                    {
                        "completed_at" => criterion.IsDescending
                            ? sortBuilder.Descending(t => t.completed_at)
                            : sortBuilder.Ascending(t => t.completed_at),

                        "amount" => criterion.IsDescending
                            ? sortBuilder.Descending(t => t.amount)
                            : sortBuilder.Ascending(t => t.amount),

                        _ => null
                    };

                    if (sortDef != null)
                        sortDefinitions.Add(sortDef);
                }

                if (sortDefinitions.Any())
                {
                    query = query.Sort(sortBuilder.Combine(sortDefinitions));
                }

                query = query
                    .Skip(creterias.Page * creterias.Limit)
                    .Limit(creterias.Limit);

                return await query.ToListAsync();
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        /// <summary>
        /// Lấy chi tiết thông tin 1 giao dịch
        /// </summary>
        public async Task<TransactionEntity> GetByIdAsync(string id)
        {
            try
            {
                var result = await _collection.Find(x => x.id == id).FirstOrDefaultAsync();
                return result;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        /// <summary>
        /// Lấy danh sách yêu cầu rút tiền (withdraw) đang chờ xử lý (Pending).
        /// </summary>
        public async Task<List<TransactionEntity>> GetPendingWithdrawRequestsAsync(FindCreterias creterias, List<SortCreterias> sortCreterias)
        {
            try
            {
                var builder = Builders<TransactionEntity>.Filter;

                var filtered = builder.And(
                    builder.Eq(x => x.type, PaymentType.WithdrawCoin),
                    builder.Eq(x => x.status, PaymentStatus.Pending)
                );

                var query = _collection
                  .Find(filtered)
                  .Skip(creterias.Page * creterias.Limit)
                  .Limit(creterias.Limit);

                var sortBuilder = Builders<TransactionEntity>.Sort;
                var sortDefinitions = new List<SortDefinition<TransactionEntity>>();

                foreach (var criterion in sortCreterias)
                {
                    SortDefinition<TransactionEntity>? sortDef = criterion.Field switch
                    {
                        "created_at" => criterion.IsDescending
                            ? sortBuilder.Descending(x => x.created_at)
                            : sortBuilder.Ascending(x => x.created_at),
                        _ => null
                    };

                    if (sortDef != null)
                        sortDefinitions.Add(sortDef);
                }

                if (sortDefinitions.Count >= 1)
                {
                    var combinedSort = sortBuilder.Combine(sortDefinitions);
                    query = query.Sort(combinedSort);
                }

                return await query.ToListAsync();
            }
            catch
            {
                throw new InternalServerException();
            }
        }
    }
}