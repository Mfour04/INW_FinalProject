using Application.Features.User.Queries;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Helpers;
using System.Text.Json.Serialization;

namespace Application.Features.Novel.Commands
{
    public class CreateNovelCommand: IRequest<ApiResponse>
    {
        [JsonPropertyName("novel")]
        public CreateNovelResponse Novel { get; set; }
    }

    public class CreateNovelHandler : IRequestHandler<CreateNovelCommand, ApiResponse>
    {
        private readonly INovelRepository _novelRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public CreateNovelHandler(INovelRepository novelRepository, IMapper mapper, IUserRepository userRepository)
        {
            _novelRepository = novelRepository;
            _mapper = mapper;
            _userRepository = userRepository;
        }
        public async Task<ApiResponse> Handle(CreateNovelCommand request, CancellationToken cancellationToken)
        {
            var author = await _userRepository.GetById(request.Novel.AuthorId);
            if(author == null)
            {
                return new ApiResponse { Success = false, Message = "Author not found" };
            }
            var novel = new NovelEntity
            {
                id = SystemHelper.RandomId(),
                title = request.Novel.Title,
                title_unsigned = SystemHelper.RemoveDiacritics(request.Novel.Title),
                description = request.Novel.Description,
                author_id = author.id,
                tags = request.Novel.Tags ?? new List<string>(),
                status = request.Novel.Status,
                is_public = request.Novel.IsPublic ?? false,
                is_paid = request.Novel.IsPaid ?? false,
                purchase_type = request.Novel.PurchaseType,
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
