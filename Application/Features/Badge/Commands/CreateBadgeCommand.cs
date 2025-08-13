using Domain.Entities;
using Domain.Enums;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Helpers;

namespace Application.Features.Badge.Commands
{
    public class CreateBadgeCommand : IRequest<ApiResponse>
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string IconUrl { get; set; }
        public int? TriggerType { get; set; }
        public int? TargetAction { get; set; }
        public int RequiredCount { get; set; }
    }

    public class CreateBadgeCommandHandler : IRequestHandler<CreateBadgeCommand, ApiResponse>
    {
        private readonly IBadgeRepository _badgeRepo;

        public CreateBadgeCommandHandler(IBadgeRepository badgeRepo)
        {
            _badgeRepo = badgeRepo;
        }

        public async Task<ApiResponse> Handle(CreateBadgeCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return Fail("Name cannot be empty.");

            if (request.TriggerType == null)
                return Fail("TriggerType is required.");

            if (request.TargetAction == null)
                return Fail("TargetAction is required.");

            if (request.RequiredCount < 0)
                return Fail("RequiredCount must be >= 0.");

            var badge = new BadgeEntity
            {
                id = SystemHelper.RandomId(),
                name = request.Name,
                description = request.Description,
                icon_url = request.IconUrl,
                trigger_type = (BadgeTriggerType)request.TriggerType,
                target_action = (BadgeAction)request.TargetAction,
                required_count = request.RequiredCount,
                created_at = TimeHelper.NowTicks
            };

            await _badgeRepo.CreateAsync(badge);

            return new ApiResponse
            {
                Success = true,
                Message = "Badge created successfully.",
                Data = badge
            };
        }

        private ApiResponse Fail(string message) => new()
        {
            Success = false,
            Message = message
        };
    }
}