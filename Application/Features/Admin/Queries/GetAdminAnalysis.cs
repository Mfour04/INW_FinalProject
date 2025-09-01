using Infrastructure.Repositories.Interfaces;
using MediatR;

namespace Shared.Contracts.Response.Admin
{
    public class GetAdminAnalysis : IRequest<ApiResponse> { }

    public class GetAdminAnalysisHandler : IRequestHandler<GetAdminAnalysis, ApiResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly INovelRepository _novelRepository;

        public GetAdminAnalysisHandler(IUserRepository userRepository, INovelRepository novelRepository)
        {
            _userRepository = userRepository;
            _novelRepository = novelRepository;
        }

        public async Task<ApiResponse> Handle(GetAdminAnalysis request, CancellationToken cancellationToken)
        {
            var totalUsersTask = _userRepository.CountAllNormalUsersAsync();
            var verifiedUsersTask = _userRepository.CountVerifiedNormalUsersAsync();
            var lockedUsersTask = _userRepository.CountLockedNormalUsersAsync();
            var totalViewsTask = _novelRepository.GetTotalViewsAsync();

            await Task.WhenAll(totalUsersTask, verifiedUsersTask, lockedUsersTask, totalViewsTask);

            var response = new AdminAnalysisResponse
            {
                TotalUsers = totalUsersTask.Result,
                VerifiedUsers = verifiedUsersTask.Result,
                LockedUsers = lockedUsersTask.Result,
                TotalNovelViews = totalViewsTask.Result
            };

            return new ApiResponse
            {
                Success = true,
                 Message = "Lấy thống kê thành công",
                Data = response
            };
        }
    }
}