using AutoMapper;
using Domain.Enums;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Report;

namespace Application.Features.Report.Queries
{
    public class GetReportById : IRequest<ApiResponse>
    {
        public string ReportId { get; set; } = default!;
    }

    public class GetReportByIdHandler : IRequestHandler<GetReportById, ApiResponse>
    {
        private readonly IReportRepository _reportRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public GetReportByIdHandler(
            IReportRepository reportRepository,
            IMapper mapper,
            IUserRepository userRepository)
        {
            _reportRepository = reportRepository;
            _mapper = mapper;
            _userRepository = userRepository;
        }

        public async Task<ApiResponse> Handle(GetReportById req, CancellationToken ct)
        {
            var r = await _reportRepository.GetByIdAsync(req.ReportId);
            if (r == null)
                return new ApiResponse { Success = false, Message = "Report not found." };

            // === load user map (reporter + moderator) ===
            var userIds = new List<string>(2);
            if (!string.IsNullOrWhiteSpace(r.reporter_id))  userIds.Add(r.reporter_id);
            if (!string.IsNullOrWhiteSpace(r.moderator_id)) userIds.Add(r.moderator_id!);

            var userMap = new Dictionary<string, BaseReportResponse.UserResponse>(StringComparer.Ordinal);
            if (userIds.Count > 0)
            {
                var users = await _userRepository.GetUsersByIdsAsync(userIds.Distinct().ToList());
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

            // === map đúng DTO theo scope + gán Reporter/Moderator ===
            BaseReportResponse dto;
            switch (r.scope)
            {
                case ReportScope.Novel:
                {
                    var mapped = _mapper.Map<NovelReportResponse>(r);
                    mapped.Reporter  = GetUser(r.reporter_id);
                    mapped.Moderator = GetUser(r.moderator_id);
                    dto = mapped;
                    break;
                }
                case ReportScope.Chapter:
                {
                    var mapped = _mapper.Map<ChapterReportResponse>(r);
                    mapped.Reporter  = GetUser(r.reporter_id);
                    mapped.Moderator = GetUser(r.moderator_id);
                    dto = mapped;
                    break;
                }
                case ReportScope.Comment:
                {
                    var mapped = _mapper.Map<CommentReportResponse>(r);
                    mapped.Reporter  = GetUser(r.reporter_id);
                    mapped.Moderator = GetUser(r.moderator_id);
                    dto = mapped;
                    break;
                }
                case ReportScope.ForumPost: 
                {
                    var mapped = _mapper.Map<ForumPostReportResponse>(r);
                    mapped.Reporter  = GetUser(r.reporter_id);
                    mapped.Moderator = GetUser(r.moderator_id);
                    dto = mapped;
                    break;
                }
                case ReportScope.ForumComment:
                {
                    var mapped = _mapper.Map<ForumCommentReportResponse>(r);
                    mapped.Reporter  = GetUser(r.reporter_id);
                    mapped.Moderator = GetUser(r.moderator_id);
                    dto = mapped;
                    break;
                }
                default:
                {
                    var mapped = _mapper.Map<BaseReportResponse>(r);
                    mapped.Reporter  = GetUser(r.reporter_id);
                    mapped.Moderator = GetUser(r.moderator_id);
                    dto = mapped;
                    break;
                }
            }

            return new ApiResponse
            {
                Success = true,
                Message = "Report retrieved successfully.",
                Data = dto
            };
        }
    }
}
