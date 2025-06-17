using Domain.Enums;
using MediatR;
using Shared.Contracts.Response;

namespace Application.Features.Badge.Queries
{
    public class GetBadgeEnums : IRequest<ApiResponse> { }

    public class GetBadgeEnumsHandler : IRequestHandler<GetBadgeEnums, ApiResponse>
    {
        public Task<ApiResponse> Handle(GetBadgeEnums request, CancellationToken cancellationToken)
        {
            var triggerTypes = Enum.GetValues(typeof(BadgeTriggerType))
                .Cast<BadgeTriggerType>()
                .Select(e => new { name = e.ToString(), value = (int)e });

            var actions = Enum.GetValues(typeof(BadgeAction))
                .Cast<BadgeAction>()
                .Select(e => new { name = e.ToString(), value = (int)e });

            return Task.FromResult(new ApiResponse
            {
                Success = true,
                Message = "Enums fetched successfully.",
                Data = new { triggerTypes, actions }
            });
        }
    }
}