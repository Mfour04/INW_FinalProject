using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;

namespace Application.Features.Badge.Commands
{
    public class DeleteBadgeCommand : IRequest<ApiResponse>
    {
        public string Id { get; set; }
    }

    public class DeleteBadgeCommandHandler : IRequestHandler<DeleteBadgeCommand, ApiResponse>
    {
        private readonly IBadgeRepository _badgeRepo;

        public DeleteBadgeCommandHandler(IBadgeRepository badgeRepo)
        {
            _badgeRepo = badgeRepo;
        }

        public async Task<ApiResponse> Handle(DeleteBadgeCommand request, CancellationToken cancellationToken)
        {
            var badge = await _badgeRepo.GetByIdAsync(request.Id);
            if (badge == null)
                return Fail("Badge not found.");

            var deleted = await _badgeRepo.DeleteAsync(request.Id);
            if (!deleted)
                return Fail("Failed to delete the post.");

            return new ApiResponse
            {
                Success = true,
                Message = "Post deleted successfully."
            };
        }

        private ApiResponse Fail(string message) => new()
        {
            Success = false,
            Message = message
        };
    }
}