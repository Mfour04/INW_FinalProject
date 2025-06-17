using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;

namespace Application.Features.Badge.Commands
{
    public class UpdateBadgeCommand : IRequest<ApiResponse>
    {
        public string? Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? IconUrl { get; set; }
        public int RequiredCount { get; set; }
    }

    public class UpdateBadgeCommandHandler : IRequestHandler<UpdateBadgeCommand, ApiResponse>
    {
        private readonly IBadgeRepository _badgeRepo;

        public UpdateBadgeCommandHandler(IBadgeRepository badgeRepo)
        {
            _badgeRepo = badgeRepo;
        }

        public async Task<ApiResponse> Handle(UpdateBadgeCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return Fail("Name cannot be empty.");

            var badge = await _badgeRepo.GetByIdAsync(request.Id);
            if (badge == null)
                return Fail("Post not found.");

            badge.name = request.Name;
            badge.required_count = request.RequiredCount;
            badge.updated_at = DateTime.Now.Ticks;

            var success = await _badgeRepo.UpdateAsync(request.Id, badge);
            if (!success)
                return Fail("Failed to update the badge.");

            return new ApiResponse
            {
                Success = true,
                Message = "Badge updated successfully."
            };
        }

        private ApiResponse Fail(string message) => new()
        {
            Success = false,
            Message = message
        };
    }
}