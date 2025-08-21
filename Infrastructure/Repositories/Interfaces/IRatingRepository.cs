using Domain.Entities;
using Domain.Entities.System;

namespace Infrastructure.Repositories.Interfaces
{
    public interface IRatingRepository
    {
        Task<List<RatingEntity>> GetByNovelIdAsync(string novelId, FindCreterias creterias, List<SortCreterias> sortCreterias);
        Task<RatingEntity> GetByIdAsync(string id);
        Task<RatingEntity> CreateAsync(RatingEntity entity);
        Task<bool> UpdateAsync(string id, RatingEntity entity);
        Task<bool> DeleteAsync(string id);
        Task<bool> HasUserRatedNovelAsync(string userId, string novelId);
        Task<double> GetAverageRatingByNovelIdAsync(string novelId);
        Task<int> GetRatingCountByNovelIdAsync(string novelId);

       Task<(IReadOnlyList<RatingEntity> items, bool hasMore)> GetByNovelIdKeysetAsync(
            string novelId,
            int limit,
            string? afterId,
            CancellationToken ct);
    }
}
