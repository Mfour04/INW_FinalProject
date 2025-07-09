using Domain.Entities;

namespace Infrastructure.Repositories.Interfaces
{
    public interface IPurchaserRepository
    {
        Task<PurchaserEntity> CreateAsync(PurchaserEntity entity);
        Task<bool> HasPurchasedFullAsync(string userId, string novelId);
        Task<bool> HasPurchasedChapterAsync(string userId, string novelId, string chapterId);
        Task<List<string>> GetPurchasedChaptersAsync(string userId, string novelId);
        Task<bool> AddChapterAsync(string userId, string novelId, string chapterId);
        Task<bool> SetFullPurchasedAsync(string userId, string novelId, int chapSnapshot);
        Task<PurchaserEntity> GetByUserAndNovelAsync(string userId, string novelId);
        Task<bool> UpdateAsync(string id, PurchaserEntity updated);
    }
}