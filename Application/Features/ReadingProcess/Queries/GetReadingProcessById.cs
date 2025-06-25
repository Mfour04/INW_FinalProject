using AutoMapper;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.ReadingProcess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.ReadingProcess.Queries
{
    public class GetReadingProcessById : IRequest<ApiResponse>
    {
        public string ReadingProcessId { get; set; }
    }

    public class GetReadingProcessByIdHandler : IRequestHandler<GetReadingProcessById, ApiResponse>
    {
        private readonly IReadingProcessRepository _readingProcessRepository;
        private readonly IMapper _mapper;

        public GetReadingProcessByIdHandler(IReadingProcessRepository readingProcessRepository, IMapper mapper)
        {
            _readingProcessRepository = readingProcessRepository;
            _mapper = mapper;
        }

        public async Task<ApiResponse> Handle(GetReadingProcessById request, CancellationToken cancellationToken)
        {
            var readingProcess = await _readingProcessRepository.GetByIdAsync(request.ReadingProcessId);
            if (readingProcess == null)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "Reading process not found."
                };
            }
            var readingProcessResponse = _mapper.Map<ReadingProcessResponse>(readingProcess);

            return new ApiResponse
            {
                Success = true,
                Message = "Reading process retrieved successfully.",
                Data = readingProcessResponse
            };
        }
    }
}
