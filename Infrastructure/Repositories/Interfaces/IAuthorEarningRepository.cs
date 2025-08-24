using Domain.Entities;

namespace Infrastructure.Repositories.Interfaces
{
    public interface IAuthorEarningRepository
    {
        Task<AuthorEarningEntity> AddAsync(AuthorEarningEntity entity);
        Task<List<AuthorEarningEntity>> GetByAuthorIdAsync(string authorId, long startTicks, long endTicks);
        Task<int> GetTotalEarningsAsync(string authorId, long startTicks, long endTicks);
        Task<List<AuthorEarningEntity>> GetByNovelAsync(string authorId, string novelId, long startTicks, long endTicks);
    }
}