using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Tag.Command
{
    public class DeleteTagCommand: IRequest<ApiResponse>
    {
        public string TagId { get; set; }
    }

    public class DeleteTagHandler : IRequestHandler<DeleteTagCommand, ApiResponse>
    {
        private readonly ITagRepository _tagRepository;

        public DeleteTagHandler(ITagRepository tagRepository)
        {
            _tagRepository = tagRepository;
        }
        public async Task<ApiResponse> Handle(DeleteTagCommand request, CancellationToken cancellationToken)
        {
            var deleted = await _tagRepository.DeleteTagAsync(request.TagId);
            return new ApiResponse
            {
                Success = true,
                Message = "Xóa tag thành công",
                Data = deleted
            };
        }
    }
}
