using Domain.Entities;
using Domain.Enums;

namespace Infrastructure.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<UserEntity> CreateUser(UserEntity entity);
        Task<UserEntity> UpdateUser(UserEntity entity);
        Task<UserEntity> GetById(string userId);
        Task<UserEntity> GetByEmail(string email);
        Task<UserEntity> GetByName(string userName);
        Task<UserEntity> GetUserNameByUserId(string userId);
        Task UpdateUserCoin(string userId, int coin, int blockedCoin);
        Task IncreaseCoinAsync(string userId, int amount);
        Task<bool> DecreaseCoinAsync(string userId, int amount);
        Task<bool> UpdateUserRoleToAdminAsync(string userId);
        Task<UserEntity?> GetFirstUserByRoleAsync(Role role);
        Task<List<UserEntity>> GetUsersByIdsAsync(List<string> userIds);
        Task<bool> IncrementFollowerCountAsync(string userId, int value);
        Task<bool> IncrementFollowingCountAsync(string userId, int value);
    }
}
