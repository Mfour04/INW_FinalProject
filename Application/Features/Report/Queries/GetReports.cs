using AutoMapper;
using Domain.Entities;
using Domain.Entities.System;
using Domain.Enums;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Report;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Report.Queries
{
    public class GetReports : IRequest<ApiResponse>
    {
        public int Page = 0;
        public int Limit = int.MaxValue;
        public string UserId { get; set; }
        public ReportTypeStatus? Type { get; set; }
        public ReportStatus? Status { get; set; }
        public string MemberId { get; set; }
        public string NovelId { get; set; }
        public string ChapterId { get; set; }
        public string CommentId { get; set; }
        public string ForumPostId { get; set; }
        public string ForumCommentId { get; set; }
    }

    public class GetReportsHandler : IRequestHandler<GetReports, ApiResponse>
    {
        private readonly IReportRepository _reportRepository;
        private readonly IMapper _mapper;

        public GetReportsHandler(IReportRepository reportRepository, IMapper mapper)
        {
            _reportRepository = reportRepository;
            _mapper = mapper;
        }

        public async Task<ApiResponse> Handle(GetReports request, CancellationToken cancellationToken)
        {
            FindCreterias findCreterias = new FindCreterias
            {
                Limit = request.Limit,
                Page = request.Page
            };

            List<ReportEntity> reports;
            // Filter theo các điều kiện
            if (!string.IsNullOrEmpty(request.UserId))
            {
                reports = await _reportRepository.GetByUserIdAsync(findCreterias, request.UserId);
            }
            else if (request.Status.HasValue)
            {
                reports = await _reportRepository.GetByStatusAsync(findCreterias, request.Status.Value);
            }
            else if (request.Type.HasValue)
            {
                reports = await _reportRepository.GetByTypeAsync(findCreterias, request.Type.Value);
            }
            else if (!string.IsNullOrEmpty(request.MemberId))
            {
                reports = await _reportRepository.GetByMemberIdAsync(findCreterias, request.MemberId);
            }
            else if (!string.IsNullOrEmpty(request.NovelId))
            {
                reports = await _reportRepository.GetByNovelIdAsync(findCreterias, request.NovelId);
            }
            else if (!string.IsNullOrEmpty(request.ChapterId))
            {
                reports = await _reportRepository.GetByChapterIdAsync(findCreterias, request.ChapterId);
            }
            else if (!string.IsNullOrEmpty(request.CommentId))
            {
                reports = await _reportRepository.GetByCommentIdAsync(findCreterias, request.CommentId);
            }
            else if (!string.IsNullOrEmpty(request.ForumPostId))
            {
                reports = await _reportRepository.GetByForumPostIdAsync(findCreterias, request.ForumPostId);
            }
            else if (!string.IsNullOrEmpty(request.ForumCommentId))
            {
                reports = await _reportRepository.GetByForumCommentIdAsync(findCreterias, request.ForumCommentId);
            }
            else
            {
                reports = await _reportRepository.GetAllAsync(findCreterias);
            }

            if (reports == null)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "No reports found."
                };
            }

            var requestResponse = _mapper.Map<List<ReportResponse>>(reports);

            return new ApiResponse
            {
                Success = true,
                Message = "Retrieved reports successfully.",
                Data = requestResponse
            };

        }
    }
}
