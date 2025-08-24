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
        public string ReportId { get; set; }
    }

    public class GetReportByIdHandler : IRequestHandler<GetReportById, ApiResponse>
    {
        private readonly IReportRepository _reportRepository;
        private readonly IMapper _mapper;
        private readonly IUserRepository _userRepository;

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
            var entity = await _reportRepository.GetByIdAsync(req.ReportId);
            if (entity == null)
                return new ApiResponse { Success = false, Message = "Report not found." };

            var userIds = new List<string>();
            if (!string.IsNullOrWhiteSpace(entity.reporter_id)) userIds.Add(entity.reporter_id);
            if (!string.IsNullOrWhiteSpace(entity.moderator_id)) userIds.Add(entity.moderator_id!);

            var userLookup = new Dictionary<string, BaseReportResponse.UserResponse>();
            if (userIds.Count > 0)
            {
                var users = await _userRepository.GetUsersByIdsAsync(userIds.Distinct().ToList());
                foreach (var u in users)
                {
                    userLookup[u.id] = new BaseReportResponse.UserResponse
                    {
                        Id = u.id,
                        Username = u.username,
                        DisplayName = u.displayname,
                        AvatarUrl = u.avata_url
                    };
                }
            }

            BaseReportResponse response;
            switch (entity.scope)
            {
                case ReportScope.Novel:
                    response = _mapper.Map<NovelReportResponse>(entity, opt => opt.Items["users"] = userLookup);
                    break;

                case ReportScope.Chapter:
                    response = _mapper.Map<ChapterReportResponse>(entity, opt => opt.Items["users"] = userLookup);
                    break;

                case ReportScope.Comment:
                    response = _mapper.Map<CommentReportResponse>(entity, opt => opt.Items["users"] = userLookup);
                    break;

                case ReportScope.ForumPost:
                    response = _mapper.Map<ForumPostReportResponse>(entity, opt => opt.Items["users"] = userLookup);
                    break;

                case ReportScope.ForumComment:
                    response = _mapper.Map<ForumCommentReportResponse>(entity, opt => opt.Items["users"] = userLookup);
                    break;

                default:
                    return new ApiResponse { Success = false, Message = "Unknown report scope." };
            }

            return new ApiResponse
            {
                Success = true,
                Message = "Report retrieved successfully.",
                Data = response
            };
        }
    }
}
