using AutoMapper;
using Domain.Entities;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Respone;
using Shared.Contracts.Response;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Novel.Commands
{
    public class CreateNovelCommand: IRequest<ApiResponse>
    {
        public CreateNovelResponse Novel { get; set; }
    }

    public class CreateNovelHandler : IRequestHandler<CreateNovelCommand, ApiResponse>
    {
        private readonly INovelRepository _novelRepository;
        private readonly IMapper _mapper;

        public CreateNovelHandler(INovelRepository novelRepository, IMapper mapper)
        {
            _novelRepository = novelRepository;
            _mapper = mapper;
        }
        public async Task<ApiResponse> Handle(CreateNovelCommand request, CancellationToken cancellationToken)
        {
            var novel = new NovelEntity
            {
                id = SystemHelper.RandomId(),
                title = request.Novel.Title,
                description = request.Novel.Description,
                author_id = request.Novel.AuthorId,
                genres = request.Novel.Genres ?? new List<string>(),
                tags = request.Novel.Tags ?? new List<string>(),
                status = request.Novel.Status,
                is_premium = request.Novel.IsPremium ?? false,
                price = request.Novel.Price ?? 0,
                created_at = DateTime.UtcNow.Ticks,
                updated_at = DateTime.UtcNow.Ticks
            };

            await _novelRepository.CreateNovelAsync(novel);
            var response = _mapper.Map<NovelResponse>(novel);

            return new ApiResponse
            {
                Success = true,
                Message = "Created Novel Successfully",
                Data = response
            };
        }
    }
}
