using AutoMapper;
using Domain.Entities;
using Domain.Entities.System;
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
    public class GetReadingHistory : IRequest<ApiResponse>
    {
        public int Page = 0;
        public int Limit = 10;
        public string? UserId { get; set; }
    }

    public class GetReadingHistoryHandler : IRequestHandler<GetReadingHistory, ApiResponse>
    {
        private readonly IReadingProcessRepository _readingProcessRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        public GetReadingHistoryHandler(
            IReadingProcessRepository readingProcessRepository,
            IUserRepository userRepository,
            IMapper mapper)
        {
            _readingProcessRepository = readingProcessRepository;
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<ApiResponse> Handle(GetReadingHistory request, CancellationToken cancellationToken)
        {
            FindCreterias findCreterias = new FindCreterias
            {
                Limit = request.Limit,
                Page = request.Page
            };
            List<ReadingProcessEntity> readingProcesses;

            if (string.IsNullOrEmpty(request.UserId))
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "UserId is required"
                };
            }
            readingProcesses = await _readingProcessRepository.GetReadingHistoryAsync(findCreterias, request.UserId);
            if (readingProcesses == null || !readingProcesses.Any())
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "No reading history found for this user"
                };
            }
            var response = _mapper.Map<List<ReadingProcessResponse>>(readingProcesses);
            return new ApiResponse
            {
                Success = true,
                Message = "Reading history retrieved successfully",
                Data = response
            };
        }
    }
}
