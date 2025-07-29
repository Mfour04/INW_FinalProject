using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Admin;
using Shared.Helpers;

namespace Application.Features.Admin.Queries
{
    public class GetAdminDashboardStats: IRequest<ApiResponse>
    {
    }
    public class GetAdminDashboardStatsHandler : IRequestHandler<GetAdminDashboardStats, ApiResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly INovelRepository _novelRepository;

        public GetAdminDashboardStatsHandler(IUserRepository userRepository, INovelRepository novelRepository)
        {
            _userRepository = userRepository;
            _novelRepository = novelRepository;
        }
        public async Task<ApiResponse> Handle(GetAdminDashboardStats request, CancellationToken cancellationToken)
        {
            var todayStart = TimeHelper.StartOfTodayTicksVN;
            var todayEnd = TimeHelper.EndOfTodayTicksVN;

            var totalUsers = await _userRepository.CountAsync();
            var newUsersToday = await _userRepository.CountAsync(u =>
                u.created_at >= todayStart && u.created_at <= todayEnd);

            var totalNovels = await _novelRepository.CountAsync();

            var newUsersPerDay = await _userRepository.CountUsersPerDayCurrentWeekAsync();
            var newNovelsPerDay = await _novelRepository.CountNovelsPerDayCurrentWeekAsync();

            var result = new AdminDashboardStatsResponse
            {
                TotalUsers = totalUsers,
                NewUsersToday = newUsersToday,
                TotalNovels = totalNovels,
                NewUsersPerDay = newUsersPerDay,
                NewNovelsPerDay = newNovelsPerDay
            };

            return new ApiResponse
            {
                Success = true,
                Message = "Lấy thống kê thành công",
                Data = result
            };
        }
    }
}
