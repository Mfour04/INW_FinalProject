using Domain.Entities;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Helpers;

namespace Application.Features.Badge.Commands
{
    public class InitBadgeProgressCommand : IRequest<ApiResponse>
    {
        public string UserId { get; set; }
    }

    public class InitBadgeProgressCommandHandler : IRequestHandler<InitBadgeProgressCommand, ApiResponse>
    {
        private readonly IBadgeRepository _badgeRepo;
        private readonly IBadgeProgressRepository _progressRepo;

        public InitBadgeProgressCommandHandler(IBadgeRepository badgeRepo, IBadgeProgressRepository progressRepo)
        {
            _badgeRepo = badgeRepo;
            _progressRepo = progressRepo;
        }

        public async Task<ApiResponse> Handle(InitBadgeProgressCommand request, CancellationToken cancellationToken)
        {
            var allBadges = await _badgeRepo.GetAllAsync(new(), new());

            if (allBadges == null || allBadges.Count == 0)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "No badges available to initialize."
                };
            }

            List<BadgeProgressEntity> toInsert = new();

            foreach (var badge in allBadges)
            {
                bool exists = await _progressRepo.ExistsAsync(request.UserId, badge.id);
                if (!exists)
                {
                    toInsert.Add(new BadgeProgressEntity
                    {
                        id = SystemHelper.RandomId(),
                        user_id = request.UserId,
                        badge_id = badge.id,
                        current_value = 0,
                        is_completed = false,
                        created_at = TimeHelper.NowTicks
                    });
                }
            }

            await _progressRepo.CreateManyAsync(toInsert);

            return new ApiResponse
            {
                Success = true,
                Message = toInsert.Count > 0
                    ? $"Initialized {toInsert.Count} badge progress entries for user." 
                    : "Badge progress already initialized for user.",
                Data = toInsert.Count
            };
        }
    }
}