using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.NovelFollower.Queries
{
    public class GetFollowerByNovelIdCommand: IRequest<ApiResponse>
    {
        public string NovelId { get; set; }
    }
    public class GetFollowerByNovelIdHanlder : IRequestHandler<GetFollowerByNovelIdCommand, ApiResponse>
    {
        private readonly INovelFollowRepository _novelFollowRepository;
        public GetFollowerByNovelIdHanlder(INovelFollowRepository novelFollowRepository)
        {
            _novelFollowRepository = novelFollowRepository;
        }

        public async Task<ApiResponse> Handle(GetFollowerByNovelIdCommand request, CancellationToken cancellationToken)
        {
            var novelFollow = await _novelFollowRepository.GetByNovelIdAsync(request.NovelId);
            if (novelFollow == null)
                return new ApiResponse { Success = false, Message = "Can not found this novel" };

            return new ApiResponse { Success = true, Data = novelFollow };
        }
    }
}
