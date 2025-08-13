using Domain.Entities;
using Domain.Entities.System;
using Domain.Enums;
using Shared.Contracts.Response.Admin;
using System.Linq.Expressions;

namespace Infrastructure.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<UserEntity> CreateUser(UserEntity entity);
        Task<UserEntity> UpdateUser(UserEntity entity);
        Task UpdateLockvsUnLockUser(string userId, bool isbanned, long? bannedUntilTicks = null);
        Task<UserEntity> GetById(string userId);
        Task<UserEntity> GetByEmail(string email);
        Task<UserEntity> GetByName(string userName);
        Task<UserEntity> GetUserNameByUserId(string userId);
        Task UpdateUserCoin(string userId, int coin, int blockedCoin);
        Task IncreaseCoinAsync(string userId, int amount);
        Task<bool> DecreaseCoinAsync(string userId, int amount);
        Task<bool> UpdateUserRoleToAdminAsync(string userId);
        Task<(List<UserEntity> Users, int TotalCount)> GetAllUserAsync(FindCreterias creterias, List<SortCreterias> sortCreterias);
        Task<UserEntity?> GetFirstUserByRoleAsync(Role role);
        Task<List<UserEntity>> GetUsersByIdsAsync(List<string> userIds);
        Task<bool> IncrementFollowerCountAsync(string userId, int value);
        Task<bool> IncrementFollowingCountAsync(string userId, int value);
        Task<int> CountAsync(Expression<Func<UserEntity, bool>> filter = null);
        Task<List<WeeklyStatItem>> CountUsersPerDayCurrentWeekAsync();
    }
}
