using AutoMapper;
using Domain.Entities;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Respone;
using Shared.Contracts.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Novel.Commands
{
    public class UpdateNovelCommand: IRequest<ApiResponse>
    {
        public UpdateNovelResponse UpdateNovel { get; set; }
    }

    public class UpdateNovelHandle : IRequestHandler<UpdateNovelCommand, ApiResponse>
    {
        private readonly INovelRepository _novelRepository;
        private readonly IMapper _mapper;

        public UpdateNovelHandle(INovelRepository novelRepository, IMapper mapper)
        {
            _novelRepository = novelRepository;
            _mapper = mapper;
        }
        public async Task<ApiResponse> Handle(UpdateNovelCommand request, CancellationToken cancellationToken)
        {
            var input = request.UpdateNovel;
            var novel = await _novelRepository.GetByNovelIdAsync(input.NovelId);
            if(novel == null)
                return new ApiResponse { Success = false, Message = "Novel not found" };

            novel.title = input.Title ?? novel.title;
            novel.description = input.Description ?? novel.description;
            novel.status = input.Status ?? novel.status;
            novel.is_premium = input.IsPremium ?? novel.is_premium;
            novel.price = input.Price ?? novel.price;
            novel.genres = input.Genres ?? novel.genres;
            novel.tags = input.Tags ?? novel.tags;
            novel.updated_at = DateTime.UtcNow.Ticks;

            await _novelRepository.UpdateNovelAsync(novel);

            var response = _mapper.Map<List<UpdateNovelResponse>>(novel);

            return new ApiResponse
            {
                Success = true,
                Message = "Novel Updated Successfullly",
                Data = response,
            };
        }
    }
}
