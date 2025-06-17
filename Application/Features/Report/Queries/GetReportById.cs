using AutoMapper;
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
    public class GetReportById : IRequest<ApiResponse>
    {
        public string ReportId { get; set; }
    }

    public class GetReportByIdHandler : IRequestHandler<GetReportById, ApiResponse>
    {
        private readonly IReportRepository _reportRepository;
        private readonly IMapper _mapper;

        public GetReportByIdHandler(IReportRepository reportRepository, IMapper mapper)
        {
            _reportRepository = reportRepository;
            _mapper = mapper;
        }

        public async Task<ApiResponse> Handle(GetReportById request, CancellationToken cancellationToken)
        {
            var report = await _reportRepository.GetByIdAsync(request.ReportId);
            if (report == null)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "Report not found."
                };
            }

            var reportResponse = _mapper.Map<ReportResponse>(report);

            return new ApiResponse
            {
                Success = true,
                Message = "Report retrieved successfully.",
                Data = reportResponse
            };
        }
    }
}
