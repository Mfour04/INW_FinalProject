using Application.Services.Interfaces;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;

namespace Application.Features.Novel.Commands
{
    public class DeleteNovelCommand: IRequest<ApiResponse>
    {
        public string NovelId { get; set; }
    }
    public class DeleteNovelHandler : IRequestHandler<DeleteNovelCommand, ApiResponse>
    {
        private readonly INovelRepository _novelRepository;
        private readonly ICurrentUserService _currentUserService;

        public DeleteNovelHandler(INovelRepository novelRepository, ICurrentUserService currentUserService)
        {
            _novelRepository = novelRepository;
            _currentUserService = currentUserService;
        }
        public async Task<ApiResponse> Handle(DeleteNovelCommand request, CancellationToken cancellationToken)
        {
            var novel = await _novelRepository.GetByNovelIdAsync(request.NovelId);
            if (novel == null)
                return new ApiResponse { Success = false, Message = "Novel not found" };

            if (novel.author_id != _currentUserService.UserId)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "Unauthorized: You are not the author of this novel"
                };
            }
            var deleted = await _novelRepository.DeleteNovelAsync(request.NovelId);

            return new ApiResponse
            {
                Success = true,
                Message = "Novel Deleted Succuessfully",
                Data = deleted
            };
        }
    }
}
