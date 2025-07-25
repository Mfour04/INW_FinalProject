using Domain.Entities;
using Domain.Entities.System;
using Domain.Enums;
using Infrastructure.InwContext;
using Infrastructure.Repositories.Interfaces;
using MongoDB.Driver;
using Shared.Exceptions;
using Shared.Helpers;

namespace Infrastructure.Repositories.Implements
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly IMongoCollection<TransactionEntity> _collection;

        public TransactionRepository(MongoDBHelper mongoDBHelper)
        {
            mongoDBHelper.CreateCollectionIfNotExistsAsync("transaction").Wait();
            _collection = mongoDBHelper.GetCollection<TransactionEntity>("transaction");
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
        /// Cập nhật trạng thái (status) của giao dịch theo id.
        /// </summary>
        public async Task<bool> UpdateStatusAsync(string id, TransactionEntity entity)
        {
            try
            {
                var filter = Builders<TransactionEntity>.Filter.Eq(x => x.id, id);

                var transaction = await _collection.Find(filter).FirstOrDefaultAsync();

                var update = Builders<TransactionEntity>
                   .Update.Set(x => x.status, entity?.status ?? transaction.status)
                   .Set(x => x.completed_at, entity.completed_at)
                   .Set(x => x.updated_at, entity.updated_at);

                await _collection.UpdateOneAsync(t => t.id == id, update);

                var updated = await _collection.FindOneAndUpdateAsync(
                   filter,
                   update,
                   new FindOneAndUpdateOptions<TransactionEntity>
                   {
                       ReturnDocument = ReturnDocument.After,
                   }
               );

                return updated != null;
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
        public async Task<(List<TransactionEntity> Transactions, int TotalCount)> GetUserTransactionsAsync(
            string userId,
            PaymentType? type,
            FindCreterias creterias,
            List<SortCreterias> sortCreterias)
        {
            try
            {
                // 1. Tạo filter cơ bản theo userId
                var builder = Builders<TransactionEntity>.Filter;
                var filter = builder.Eq(t => t.requester_id, userId);

                // 2. Nếu có filter theo type thì thêm vào
                if (type.HasValue)
                    filter &= builder.Eq(t => t.type, type.Value);

                // 3. Xây dựng phần sort
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

                // Nếu không có sort cụ thể → fallback sort theo created_at desc
                var combinedSort = sortDefinitions.Any()
                    ? sortBuilder.Combine(sortDefinitions)
                    : sortBuilder.Descending(t => t.created_at);

                // 4. Tạo 2 task song song: count và get data
                var countTask = _collection.CountDocumentsAsync(filter);

                var dataTask = _collection.Find(filter)
                    .Sort(combinedSort)
                    .Skip(creterias.Page * creterias.Limit)
                    .Limit(creterias.Limit)
                    .ToListAsync();

                // 5. Chạy song song, chờ cả 2 task hoàn thành
                await Task.WhenAll(countTask, dataTask);

                // 6. Trả tuple gồm danh sách và tổng số record
                return (dataTask.Result, (int)countTask.Result);
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