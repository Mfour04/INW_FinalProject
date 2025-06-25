using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.ReadingProcess.Command
{
    public class DeleteReadingProcessCommand : IRequest<ApiResponse>
    {
        public string ReadingProcessId { get; set; }
    }
    public class DeleteReadingProcessCommandHandler : IRequestHandler<DeleteReadingProcessCommand, ApiResponse>
    {
        private readonly IReadingProcessRepository _readingProcessRepository;

        public DeleteReadingProcessCommandHandler(IReadingProcessRepository readingProcessRepository)
        {
            _readingProcessRepository = readingProcessRepository;
        }

        public async Task<ApiResponse> Handle(DeleteReadingProcessCommand request, CancellationToken cancellationToken)
        {
            var deleted = await _readingProcessRepository.DeleteAsync(request.ReadingProcessId);
            if (deleted == null)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "Reading process not found or already deleted"
                };
            }
            return new ApiResponse
            {
                Success = true,
                Message = "Reading process deleted successfully",
                Data = deleted
            };
        }
    }
}
