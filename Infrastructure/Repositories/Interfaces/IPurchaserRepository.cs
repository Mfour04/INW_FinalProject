using Domain.Entities;

namespace Infrastructure.Repositories.Interfaces
{
    public interface IPurchaserRepository
    {
        Task<bool> HasPurchasedFullAsync(string userId, string novelId);
        Task<int> CountPurchasedChaptersAsync(string userId, string novelId);
        Task<bool> HasPurchasedChapterAsync(string userId, string novelId, string chapterId);
        Task AddChapterPurchaseAsync(string userId, string novelId, string chapterId);
        Task AddFullNovelPurchaseAsync(PurchaserEntity entity);
        Task<bool> UpdatePurchaseStatusAsync(string id, bool isFull, int fullChapterCount);
        Task TryMarkFullAsync(string userId, string novelId, int totalChapters);
        Task ValidateFullPurchaseAsync(string userId, string novelId, int currentTotalChapters);
        Task<List<string>> GetPurchasedChapterIdsAsync(string userId, string novelId);
    }
}