using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.NovelFollower.Commands
{
    public class DeleteNovelFollowerCommand: IRequest<ApiResponse>
    {
        public string NovelFollowerId { get; set; }
    }

    public class DeleteNovelFollowerHanlder : IRequestHandler<DeleteNovelFollowerCommand, ApiResponse>
    {
        private readonly INovelFollowRepository _novelFollowRepository;

        private readonly INovelRepository _novelRepository;
        public DeleteNovelFollowerHanlder(INovelFollowRepository novelFollowRepository, INovelRepository novelRepository)
        {
            _novelFollowRepository = novelFollowRepository;
            _novelRepository = novelRepository;
        }
        public async Task<ApiResponse> Handle(DeleteNovelFollowerCommand request, CancellationToken cancellationToken)
        {
            var follow = await _novelFollowRepository.GetByNovelFollowIdAsync(request.NovelFollowerId);
            if(follow == null)
                return new ApiResponse { Success = false, Message = "Không tìm thấy theo dõi này" };

            var deleted = await _novelFollowRepository.DeleteNovelFollowAsync(follow.id);
            await _novelRepository.DecrementFollowersAsync(follow.novel_id);
            return new ApiResponse 
            { 
                Success = true, 
                Message = "Novel Follower deleted succesfully", 
                Data = deleted
            };
        }
    }
}
