using Domain.Entities;
using Infrastructure.Repositories.Implements;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Chapter.Queries
{
    public class GetChapterById: IRequest<ApiResponse>
    {
        public string ChapterId { get; set; }
        public string UserId { get; set; }
        public string NovelId { get; set; }
    }
    public class GetChapterByIdHandler : IRequestHandler<GetChapterById, ApiResponse>
    {
        private readonly IChapterRepository _chapterRepository;
        private readonly IPurchaserRepository _purchaserRepository;
        private readonly INovelRepository _novelRepository;
        public GetChapterByIdHandler(IChapterRepository chapterRepository, IPurchaserRepository purchaserRepository, INovelRepository novelRepository)
        {
            _chapterRepository = chapterRepository;
            _purchaserRepository = purchaserRepository;
            _novelRepository = novelRepository;
        }

        public async Task<ApiResponse> Handle(GetChapterById request, CancellationToken cancellationToken)
        {
            var chapter = await _chapterRepository.GetByChapterIdAsync(request.ChapterId);
            if (chapter == null)
                return new ApiResponse { Success = false, Message = "Chapter not found" };

            //var novel = await _novelRepository.GetByNovelIdAsync(request.NovelId);    
            //if (novel == null)
            //    return new ApiResponse { Success = false, Message = "Novel not found" };

            if (!chapter.is_paid)
                return new ApiResponse { Success = true, Data = chapter };

            if (chapter.is_paid)
            {
                //bool isAuthor = novel.author_id == request.UserId;
                bool hasFullOwnerShip = await _purchaserRepository.HasPurchasedFullAsync(request.UserId, request.NovelId);
                bool hasFullChapter = await _purchaserRepository.HasPurchasedChapterAsync(request.UserId, request.NovelId, request.ChapterId);
                if ( hasFullOwnerShip || hasFullChapter)
                {
                    return new ApiResponse { Success = true, Data = chapter };
                }
                else
                {
                    return new ApiResponse { Success = false, Message = "Bạn chưa mua chương này." };
                }

            }
            return new ApiResponse { Success = false, Message = "Bạn không có quyền xem chương này." };
        }
    }
}
