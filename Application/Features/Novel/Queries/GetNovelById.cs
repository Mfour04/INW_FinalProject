using Application.Features.User.Queries;
using Domain.Enums;
using Infrastructure.Repositories.Implements;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;

namespace Application.Features.Novel.Queries
{
    public class GetNovelById: IRequest<ApiResponse>
    {
        public string NovelId { get; set; }
        public string UserId { get; set; }
        public string ChapterId { get; set; }
    }
    public class GetNovelHanlder : IRequestHandler<GetNovelById, ApiResponse>
    {
        private readonly INovelRepository _novelRepository;
        private readonly IOwnershipRepository _ownershipRepository;
        private readonly IChapterRepository _chapterRepository;
        public GetNovelHanlder(INovelRepository novelRepository, IOwnershipRepository ownershipRepository, IChapterRepository chapterRepository)
        {
            _novelRepository = novelRepository;
            _ownershipRepository = ownershipRepository;
            _chapterRepository = chapterRepository;
        }
        public async Task<ApiResponse> Handle(GetNovelById request, CancellationToken cancellationToken)
        {
            try
            {
                var novel = await _novelRepository.GetByNovelIdAsync(request.NovelId);
                var chapter = await _chapterRepository.GetChaptersByNovelIdAsync(request.NovelId);
                bool isAuthor = novel.author_id == request.UserId;
                bool hasFullOwnership = await _ownershipRepository.HasFullNovelOwnershipAsync(request.UserId, request.NovelId);
                bool hasAnyChapterOwnership = await _ownershipRepository.HasAnyChapterOwnershipAsync(request.UserId, request.NovelId);
                var ownedChapterIds = await _ownershipRepository.GetOwnedChapterIdsAsync(request.UserId, request.NovelId);
                var freeChapters = _chapterRepository.GetFreeChaptersByNovelIdAsync(request.NovelId);

                if (novel == null)
                    return new ApiResponse { Success = false, Message = "Novel not found" };

                if(!novel.is_paid && novel.is_public)
                {
                    var result = new
                    {
                        Novel = novel,
                        Chapters = chapter
                    };
                    return new ApiResponse { Success = true, Data = result };
                }
                                 
                if (novel.is_paid)
                {
                    if (novel.status == NovelStatus.Completed)
                    {
                        if (!isAuthor && !hasFullOwnership)
                        {
                            var result = new
                            {
                                Novel = novel,
                                Chapters = freeChapters,
                                Message = "Bạn chưa mua truyện này (đã hoàn thành). Chỉ xem được chương miễn phí."
                            };
                            return new ApiResponse { Success = true, Data = result };
                        }
                    }
                    else if (novel.status == NovelStatus.Ongoing)
                    {
                        if (!isAuthor && !hasAnyChapterOwnership)
                        {                         
                            var result = new
                            {
                                Novel = novel,
                                Chapters = freeChapters,
                                Message = "Bạn chưa mua chương nào trong truyện này (đang ra). Chỉ xem được chương miễn phí."
                            };
                            return new ApiResponse { Success = true, Data = result };
                        }
                           
                    }
                }

                if (!novel.is_public)
                {
                    if (novel.author_id != request.UserId && !hasFullOwnership)
                    {
                        return new ApiResponse { Success = false, Message = "Truyện này chưa được công khai." };
                    }
                }

                var fullResult = new
                {
                    Novel = novel,
                    Chapters = chapter
                };

                return new ApiResponse { Success = true, Data = fullResult };
            }
            catch (Exception ex)
            {
                return new ApiResponse { Success = false, Message = $"An error occurred: {ex.Message}" };
            }
        }
    }
}
