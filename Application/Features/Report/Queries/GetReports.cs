using AutoMapper;
using Domain.Entities.System;
using Domain.Enums;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Report;
using Shared.Helpers;

namespace Application.Features.Report.Queries
{
    public class GetReports : IRequest<ApiResponse>
    {
        public string SortBy { get; set; } = "created_at:desc";
        public int Page { get; set; } = 0;
        public int Limit { get; set; } = int.MaxValue;

        public ReportScope? Scope { get; set; }
        public ReportStatus? Status { get; set; }
    }

    public class GetReportsHandler : IRequestHandler<GetReports, ApiResponse>
    {
        private readonly IReportRepository _reportRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public GetReportsHandler(
            IReportRepository reportRepository,
            IMapper mapper,
            IUserRepository userRepository)
        {
            _reportRepository = reportRepository;
            _mapper = mapper;
            _userRepository = userRepository;
        }

        public async Task<ApiResponse> Handle(GetReports request, CancellationToken cancellationToken)
        {
            var find = new FindCreterias
            {
                Limit = request.Limit,
                Page = request.Page
            };
            var sort = SystemHelper.ParseSortCriteria(request.SortBy);

            var reports = await _reportRepository.GetAllAsync(request.Scope, request.Status, find, sort);

            var totalCount = await _reportRepository.CountAsync(request.Scope, request.Status);

            var userIds = reports
                .Select(r => r.reporter_id)
                .Concat(reports.Where(r => !string.IsNullOrWhiteSpace(r.moderator_id))
                .Select(r => r.moderator_id!))
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToList();

            var userMap = new Dictionary<string, BaseReportResponse.UserResponse>(StringComparer.Ordinal);
            if (userIds.Count > 0)
            {
                var users = await _userRepository.GetUsersByIdsAsync(userIds);
                foreach (var u in users)
                {
                    userMap[u.id] = new BaseReportResponse.UserResponse
                    {
                        Id = u.id,
                        Username = u.username,
                        DisplayName = u.displayname,
                        AvatarUrl = u.avata_url
                    };
                }
            }

            BaseReportResponse.UserResponse? GetUser(string? id)
                => !string.IsNullOrWhiteSpace(id) && userMap.TryGetValue(id, out var v) ? v : null;

            var response = reports.Select(r =>
            {
                BaseReportResponse dto;
                switch (r.scope)
                {
                    case ReportScope.Novel:
                        {
                            var mapped = _mapper.Map<NovelReportResponse>(r);
                            mapped.Reporter = GetUser(r.reporter_id);
                            mapped.Moderator = GetUser(r.moderator_id);
                            dto = mapped;
                            break;
                        }
                    case ReportScope.Chapter:
                        {
                            var mapped = _mapper.Map<ChapterReportResponse>(r);
                            mapped.Reporter = GetUser(r.reporter_id);
                            mapped.Moderator = GetUser(r.moderator_id);
                            dto = mapped;
                            break;
                        }
                    case ReportScope.Comment:
                        {
                            var mapped = _mapper.Map<CommentReportResponse>(r);
                            mapped.Reporter = GetUser(r.reporter_id);
                            mapped.Moderator = GetUser(r.moderator_id);
                            dto = mapped;
                            break;
                        }
                    case ReportScope.ForumPost:
                        {
                            var mapped = _mapper.Map<ForumPostReportResponse>(r);
                            mapped.Reporter = GetUser(r.reporter_id);
                            mapped.Moderator = GetUser(r.moderator_id);
                            dto = mapped;
                            break;
                        }
                    case ReportScope.ForumComment:
                        {
                            var mapped = _mapper.Map<ForumCommentReportResponse>(r);
                            mapped.Reporter = GetUser(r.reporter_id);
                            mapped.Moderator = GetUser(r.moderator_id);
                            dto = mapped;
                            break;
                        }
                    default:
                        {
                            var mapped = _mapper.Map<BaseReportResponse>(r);
                            mapped.Reporter = GetUser(r.reporter_id);
                            mapped.Moderator = GetUser(r.moderator_id);
                            dto = mapped;
                            break;
                        }
                }
                return dto;
            }).ToList();

            var limit = request.Limit <= 0 ? int.MaxValue : request.Limit;
            var totalPages = totalCount == 0
                ? 0
                : (limit == int.MaxValue
                    ? 1
                    : (int)Math.Ceiling(totalCount / (double)limit));

            return new ApiResponse
            {
                Success = true,
                Message = response.Count == 0 ? "No report found." : "Retrieved reports successfully.",
                Data = new
                {
                    Reports = response,
                    TotalReports = totalCount,
                    TotalPages = totalPages
                }
            };
        }
    }
}
