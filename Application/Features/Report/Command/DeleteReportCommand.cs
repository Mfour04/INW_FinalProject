using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Report.Command
{
    public class DeleteReportCommand : IRequest<ApiResponse>
    {
        public string ReportId { get; set; }
    }

    public class DeleteReportCommandHandler : IRequestHandler<DeleteReportCommand, ApiResponse>
    {
        private readonly IReportRepository _reportRepository;

        public DeleteReportCommandHandler(IReportRepository reportRepository)
        {
            _reportRepository = reportRepository;
        }

        public async Task<ApiResponse> Handle(DeleteReportCommand request, CancellationToken cancellationToken)
        {
            var deleted = await _reportRepository.DeleteAsync(request.ReportId);
            if (deleted == null)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "Report not found or already deleted"
                };
            }
            return new ApiResponse
            {
                Success = true,
                Message = "Report Deleted Successfully",
                Data = deleted
            };
        }
    }
}
