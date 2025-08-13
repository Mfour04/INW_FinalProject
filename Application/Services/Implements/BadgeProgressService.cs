using Application.Services.Interfaces;
using Domain.Entities;
using Infrastructure.Repositories.Interfaces;
using Shared.Helpers;

namespace Application.Services.Implements
{
    public class BadgeProgressService : IBadgeProgressService
    {
        private readonly IBadgeRepository _badgeRepo;
        private readonly IBadgeProgressRepository _progressRepo;

        public BadgeProgressService(
            IBadgeRepository badgeRepo,
            IBadgeProgressRepository progressRepo)
        {
            _badgeRepo = badgeRepo;
            _progressRepo = progressRepo;
        }

        public async Task InitializeUserBadgeProgress(string userId)
        {
            var allBadges = await _badgeRepo.GetAllWithoutPagingAsync();

            if (allBadges == null || allBadges.Count == 0)
                return;

            List<BadgeProgressEntity> toInsert = new();

            foreach (var badge in allBadges)
            {
                bool exists = await _progressRepo.ExistsAsync(userId, badge.id);
                if (!exists)
                {
                    toInsert.Add(new BadgeProgressEntity
                    {
                        id = SystemHelper.RandomId(),
                        user_id = userId,
                        badge_id = badge.id,
                        current_value = 0,
                        is_completed = false,
                        created_at = TimeHelper.NowTicks
                    });
                }
            }

            if (toInsert.Count > 0)
                await _progressRepo.CreateManyAsync(toInsert);
        }
    }
}