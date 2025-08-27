using AutoMapper;
using Domain.Enums;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.NovelFollow;

namespace Application.Features.NovelFollower.Commands
{
    public class UpdateNovelFollowCommand: IRequest<ApiResponse>
    {
        public string NovelFollowId { get; set; }
        public bool IsNotification { get; set; }
        public NovelFollowReadingStatus ReadingStatus { get; set; }
    }
    public class UpdateNovelFollowHandler : IRequestHandler<UpdateNovelFollowCommand, ApiResponse>
    {
        private readonly INovelFollowRepository _novelFollowRepository;
        private readonly IMapper _mapper;
        public UpdateNovelFollowHandler(INovelFollowRepository novelFollowRepository, IMapper mapper)
        {
            _novelFollowRepository = novelFollowRepository;
            _mapper = mapper;
        }
        public async Task<ApiResponse> Handle(UpdateNovelFollowCommand request, CancellationToken cancellationToken)
        {
            var novelFollow = await _novelFollowRepository.GetByNovelFollowIdAsync(request.NovelFollowId);
            if (novelFollow == null)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "Không tìm thấy Novel follow."
                };
            }
            novelFollow.is_notification = request.IsNotification;
            novelFollow.reading_status = request.ReadingStatus;
            var updateResult = await _novelFollowRepository.UpdateNovelFollowAsync(novelFollow);
                var response = _mapper.Map<UpdateNovelFollowResponse>(updateResult);
                return new ApiResponse
                {
                    Success = true,
                    Message = "Novel follow được cập nhập thành công.",
                    Data = response
                };           
        }
    }
}
