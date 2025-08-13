using Domain.Entities;

namespace Infrastructure.Repositories.Interfaces
{
    public interface IUserBankAccountRepository
    {
        Task<UserBankAccountEntity> GetByIdAsync(string id);
        Task<List<UserBankAccountEntity>> GetByIdsAsync(List<string> ids);
        Task<List<UserBankAccountEntity>> GetByUserAsync(string userId);
        Task AddAsync(UserBankAccountEntity entity);
        Task<bool> DeleteAsync(string id);
        Task<bool> SetDefaultAsync(string userId, string accountId);
    }
}