using AutoMapper;
using Domain.Enums;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using MongoDB.Driver;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Report;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Report.Command
{
    public class UpdateReportCommand : IRequest<ApiResponse>
    {
        public List<string> ReportIds { get; set; }
        public ReportStatus Status { get; set; }
    }

    public class UpdateReportCommandHandler : IRequestHandler<UpdateReportCommand, ApiResponse>
    {
        private readonly IReportRepository _reportRepository;

        public UpdateReportCommandHandler(IReportRepository reportRepository)
        {
            _reportRepository = reportRepository;
        }

        public async Task<ApiResponse> Handle(UpdateReportCommand request, CancellationToken cancellationToken)
        {
            var input = request;
            var report = await _reportRepository.GetManyByIdsAsync(input.ReportIds);
            if (report == null)
                return new ApiResponse { Success = false, Message = "Report not found" };

            var updatedReport = await _reportRepository.UpdateManyAsync(request.ReportIds, request.Status);
            if (updatedReport.Count == 0)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "Không có report nào được cập nhật"
                };
            }

            return new ApiResponse
            {
                Success = true,
                Message = "Report updated successfully",
            };
        }
    }
}
