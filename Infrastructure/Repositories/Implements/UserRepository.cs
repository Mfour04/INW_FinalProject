using Domain.Entities;
using Domain.Entities.System;
using Domain.Enums;
using Infrastructure.InwContext;
using Infrastructure.Repositories.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;
using Shared.Contracts.Response.Admin;
using Shared.Exceptions;
using Shared.Helpers;
using System.Linq.Expressions;

namespace Infrastructure.Repositories.Implements
{
    public class UserRepository : IUserRepository
    {
        private readonly IMongoCollection<UserEntity> _collection;

        public UserRepository(MongoDBHelper mongoDBHelper)
        {
            // Tạo collection nếu chưa có
            mongoDBHelper.CreateCollectionIfNotExistsAsync("user").Wait();

            // Gán collection
            _collection = mongoDBHelper.GetCollection<UserEntity>("user");
        }

        /// <summary>
        /// Tạo người dùng mới
        /// </summary>
        public async Task<UserEntity> CreateUser(UserEntity entity)
        {
            try
            {
                entity.displayname_normalized = SystemHelper.RemoveDiacritics(entity.displayname);
                await _collection.InsertOneAsync(entity);
                return entity;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        /// <summary>
        /// Cập nhật thông tin người dùng
        /// </summary>
        public async Task<UserEntity> UpdateUser(UserEntity entity)
        {
            try
            {
                var filter = Builders<UserEntity>.Filter.Eq(x => x.id, entity.id);
                await _collection.ReplaceOneAsync(filter, entity);
                return entity;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        /// <summary>
        /// Lấy người dùng theo ID
        /// </summary>
        public async Task<UserEntity> GetById(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("userId must not be null or empty");
            try
            {
                return await _collection.Find(x => x.id == userId).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                throw new InternalServerException("Error getting user by id: " + ex.Message);
            }
        }

        /// <summary>
        /// Lấy người dùng theo email
        /// </summary>
        public async Task<UserEntity> GetByEmail(string email)
        {
            try
            {
                var result = await _collection.Find(x => x.email == email).FirstOrDefaultAsync();
                return result;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        /// <summary>
        /// Lấy người dùng theo username
        /// </summary>
        public async Task<UserEntity> GetByName(string userName)
        {
            try
            {
                var result = await _collection.Find(x => x.username == userName).FirstOrDefaultAsync();
                return result;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        /// <summary>
        /// Cập nhật coin và coin bị khóa
        /// </summary>
        public async Task UpdateUserCoin(string userId, int coin, int blockedCoin)
        {
            var filter = Builders<UserEntity>.Filter.Eq(x => x.id, userId);

            var update = Builders<UserEntity>.Update
                .Set(x => x.coin, coin)
                .Set(x => x.block_coin, blockedCoin)
                .Set(x => x.updated_at, DateTime.Now.Ticks);

            await _collection.UpdateOneAsync(filter, update);
        }

        /// <summary>
        /// Tăng số coin
        /// </summary>
        public async Task IncreaseCoinAsync(string userId, int amount)
        {
            try
            {
                var filter = Builders<UserEntity>.Filter.Eq(u => u.id, userId);
                var update = Builders<UserEntity>.Update.Inc(u => u.coin, amount);

                var result = await _collection.UpdateOneAsync(filter, update);
            }
            catch
            {
                throw new InternalServerException();

            }
        }

        /// <summary>
        /// Giảm số coin (nếu đủ)
        /// </summary>
        public async Task<bool> DecreaseCoinAsync(string userId, int amount)
        {
            try
            {
                var filter = Builders<UserEntity>.Filter.And(
                    Builders<UserEntity>.Filter.Eq(u => u.id, userId),
                    Builders<UserEntity>.Filter.Gte(u => u.coin, amount)
                );

                var update = Builders<UserEntity>.Update.Inc(u => u.coin, -amount);

                var result = await _collection.UpdateOneAsync(filter, update);

                return result.ModifiedCount > 0;
            }
            catch (Exception)
            {
                throw new InternalServerException();
            }
        }

        /// <summary>
        /// Cập nhật vai trò người dùng thành admin
        /// </summary>
        public async Task<bool> UpdateUserRoleToAdminAsync(string userId)
        {
            var filter = Builders<UserEntity>.Filter.Eq(u => u.id, userId);
            var update = Builders<UserEntity>.Update.Set(nameof(UserEntity.role), Role.Admin);

            var result = await _collection.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        /// <summary>
        /// Lấy người dùng đầu tiên theo role
        /// </summary>
        public async Task<UserEntity?> GetFirstUserByRoleAsync(Role role)
        {
            var filter = Builders<UserEntity>.Filter.Eq(nameof(UserEntity.role), role);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Lấy username theo userId
        /// </summary>
        public async Task<UserEntity> GetUserNameByUserId(string userId)
        {
            try
            {
                var filter = Builders<UserEntity>.Filter.Eq(x => x.id, userId);
                var projection = Builders<UserEntity>.Projection.Include(x => x.username);
                var result = await _collection.Find(filter).Project<UserEntity>(projection).FirstOrDefaultAsync();

                return result;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        /// <summary>
        /// Lấy danh sách người dùng theo nhiều userId
        /// </summary>
        public async Task<List<UserEntity>> GetUsersByIdsAsync(List<string> userIds)
        {
            try
            {
                var filter = Builders<UserEntity>.Filter.In(u => u.id, userIds);
                var result = await _collection.Find(filter).ToListAsync();
                return result;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        /// <summary>
        /// Tăng/giảm số lượng người theo dõi (follower)
        /// </summary>
        public async Task<bool> IncrementFollowerCountAsync(string userId, int value)
        {
            try
            {
                var update = Builders<UserEntity>.Update.Inc(x => x.follower_count, value);
                var result = await _collection.UpdateOneAsync(x => x.id == userId, update);
                return result.ModifiedCount > 0;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        /// <summary>
        /// Tăng/giảm số lượng người đang theo dõi (following)
        /// </summary>
        public async Task<bool> IncrementFollowingCountAsync(string userId, int value)
        {
            try
            {
                var update = Builders<UserEntity>.Update.Inc(x => x.following_count, value);
                var result = await _collection.UpdateOneAsync(x => x.id == userId, update);
                return result.ModifiedCount > 0;
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<(List<UserEntity> Users, int TotalCount)> GetAllUserAsync(FindCreterias creterias, List<SortCreterias> sortCreterias)
        {
            try
            {
                var builder = Builders<UserEntity>.Filter;
                var filtered = builder.Empty;
                var isExact = true;
                if (creterias.SearchTerm != null && creterias.SearchTerm.Count > 0)
                {
                    if (creterias.SearchTerm.Count == 1)
                    {
                        var keyword = creterias.SearchTerm[0];

                        // ✅ THAY builder.Eq thành Regex để chứa từ đó (fuzzy nhẹ)
                        filtered &= builder.Regex(
                            x => x.displayname_unsigned,
                            new BsonRegularExpression($".*{keyword}.*", "i")
                        );
                    }
                    else
                    {
                        // Fuzzy match: tất cả từ phải khớp
                        var regexFilters = creterias.SearchTerm.Select(term =>
                            builder.Regex(x => x.displayname_unsigned, new BsonRegularExpression($".*{SystemHelper.RemoveDiacritics(term).ToLower()}.*", "i"))
                        );
                        filtered &= builder.And(regexFilters);
                    }
                }

                var query = _collection
                    .Find(filtered)
                    .Skip(creterias.Page * creterias.Limit)
                    .Limit(creterias.Limit);

                var sortBuilder = Builders<UserEntity>.Sort;
                var sortDefinitions = new List<SortDefinition<UserEntity>>();
                var totalCount = await _collection.CountDocumentsAsync(filtered);
                foreach (var criterion in sortCreterias)
                {
                    SortDefinition<UserEntity>? sortDef = criterion.Field switch
                    {
                        "created_at" => criterion.IsDescending
                            ? sortBuilder.Descending(x => x.created_at)
                            : sortBuilder.Ascending(x => x.created_at),
                        "displayname_normalized" => criterion.IsDescending
                            ? sortBuilder.Descending(x => x.displayname_normalized)
                            : sortBuilder.Ascending(x => x.displayname_normalized),
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
                var novels = await query.ToListAsync();
                return (novels, (int)totalCount);
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task UpdateLockvsUnLockUser(string userId, bool isbanned, long? bannedUntilTicks = null)
        {
            try
            {
                var filter = Builders<UserEntity>.Filter.Eq(x => x.id, userId);
                var updatelockUser = Builders<UserEntity>.Update
                   .Set(x => x.is_banned, isbanned)
                   .Set(x => x.banned_until, bannedUntilTicks);
                await _collection.UpdateOneAsync(filter, updatelockUser);
            }
            catch
            {
                throw new InternalServerException();
            }
        }

        public async Task<int> CountAsync(Expression<Func<UserEntity, bool>> filter = null)
        {
            return (int)(filter == null
            ? await _collection.CountDocumentsAsync(_ => true)
            : await _collection.CountDocumentsAsync(filter));
        }

        public async Task<List<WeeklyStatItem>> CountUsersPerDayCurrentWeekAsync()
        {
            var fromTicks = TimeHelper.StartOfCurrentWeekTicksVN;
            var toTicks = TimeHelper.NowTicks;

            var users = await _collection
                .Find(u => u.created_at >= fromTicks && u.created_at <= toTicks)
                .ToListAsync();

            // Nhóm theo ngày giờ Việt Nam
            var grouped = users
                .GroupBy(u => TimeHelper.ToVN(new DateTime(u.created_at)).Date)
                .ToDictionary(g => g.Key, g => g.Count());

            // Lấy danh sách các ngày từ thứ 2 đến hôm nay
            var days = TimeHelper.GetDaysFromStartOfWeekToTodayVN();

            var result = days.Select(d => new WeeklyStatItem
            {
                Day = d.ToString("yyyy-MM-dd"),
                Count = grouped.ContainsKey(d) ? grouped[d] : 0,
                Weekday = TimeHelper.DayOfWeekVN(d.DayOfWeek)
            }).ToList();

            return result;
        }
    }
}
