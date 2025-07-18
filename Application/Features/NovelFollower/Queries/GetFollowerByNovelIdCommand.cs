using AutoMapper;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Chapter;
using Shared.Contracts.Response.Follow;
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
        private readonly IMapper _mapper;
        public GetFollowerByNovelIdHanlder(INovelFollowRepository novelFollowRepository, IMapper mapper)
        {
            _novelFollowRepository = novelFollowRepository;
            _mapper = mapper;
        }

        public async Task<ApiResponse> Handle(GetFollowerByNovelIdCommand request, CancellationToken cancellationToken)
        {
            var novelFollow = await _novelFollowRepository.GetByNovelIdAsync(request.NovelId);
            if (novelFollow == null)
                return new ApiResponse { Success = false, Message = "Can not found this novel." };
            var novelFollowReponse = _mapper.Map<List<NovelFollowResponse>>(novelFollow);
            return new ApiResponse { Success = true, Data = novelFollowReponse };
        }
    }
}
