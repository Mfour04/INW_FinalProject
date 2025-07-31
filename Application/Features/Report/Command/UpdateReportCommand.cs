using AutoMapper;
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

namespace Application.Features.Report.Command
{
    public class UpdateReportCommand : IRequest<ApiResponse>
    {
        public string ReportId { get; set; }
        public ReportStatus Status { get; set; }
    }

    public class UpdateReportCommandHandler : IRequestHandler<UpdateReportCommand, ApiResponse>
    {
        private readonly IReportRepository _reportRepository;
        private readonly IMapper _mapper;
        public UpdateReportCommandHandler(IReportRepository reportRepository, IMapper mapper)
        {
            _reportRepository = reportRepository;
            _mapper = mapper;
        }
        public async Task<ApiResponse> Handle(UpdateReportCommand request, CancellationToken cancellationToken)
        {
            var input = request;
            var report = await _reportRepository.GetByIdAsync(input.ReportId);
            if (report == null)
                return new ApiResponse { Success = false, Message = "Report not found" };
            report.status = input.Status;
            var updatedReport = await _reportRepository.UpdateAsync(report);
            var response = _mapper.Map<UpdateReportResponse>(updatedReport);
            return new ApiResponse
            {
                Success = true,
                Message = "Report updated successfully",
                Data = response
            };
        }
    }
}
