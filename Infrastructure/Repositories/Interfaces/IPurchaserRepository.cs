using Domain.Entities;

namespace Infrastructure.Repositories.Interfaces
{
    public interface IPurchaserRepository
    {
        Task<PurchaserEntity> PurchasedFullAsync(PurchaserEntity entity);
        Task<bool> HasPurchasedFullAsync(string userId, string novelId);
        Task<int> CountPurchasedChaptersAsync(string userId, string novelId);
        Task<bool> HasPurchasedChapterAsync(string userId, string novelId, string chapterId);
        Task PurchasedChapterAsync(string userId, string novelId, string chapterId);
        Task TryMarkFullAsync(string userId, string novelId);
    }
}