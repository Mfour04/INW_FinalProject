using Infrastructure.Repositories.Implements;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Novel;
using Shared.Contracts.Response.Tag;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Novel.Queries
{
    public class GetRecommendByNovelId: IRequest<ApiResponse>
    {
        public string NovelId { get; set; }
        public int TopN { get; set; }  
    }
    public class GetRecommendByNovelIdHandler : IRequestHandler<GetRecommendByNovelId, ApiResponse>
    {
        private readonly IOpenAIRepository _openAIRepository;
        private readonly INovelRepository _novelRepository;
        private readonly IUserRepository _userRepository;
        private readonly ITagRepository _tagRepository;
        public GetRecommendByNovelIdHandler(IOpenAIRepository openAIRepository, ITagRepository tagRepository
            ,IUserRepository userRepository, INovelRepository novelRepository)
        {
            _openAIRepository = openAIRepository;
            _tagRepository = tagRepository;
            _userRepository = userRepository;
            _novelRepository = novelRepository;
        }
        public async Task<ApiResponse> Handle(GetRecommendByNovelId request, CancellationToken cancellationToken)
        {

            var novelEmbeddingEntity = await _openAIRepository.GetNovelEmbeddingAsync(request.NovelId);
            if (novelEmbeddingEntity == null || novelEmbeddingEntity.vector_novel == null)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "Novel embedding not found."
                };
            }

            // 1. Lấy NovelId + Score
            var similarResults = await _openAIRepository.GetSimilarNovelsAsync(
                novelEmbeddingEntity.vector_novel, request.TopN, request.NovelId);

            var novelIds = similarResults.Select(x => x.NovelId).ToList();
            var scoreDict = similarResults.ToDictionary(x => x.NovelId, x => x.Score);

            // 2. Lấy chi tiết novel
            var novels = await _novelRepository.GetManyByIdsAsync(novelIds);

            // 3. Lấy thông tin tác giả và tag
            var authorIds = novels.Select(n => n.author_id).Distinct().ToList();
            var tagIds = novels.SelectMany(n => n.tags).Distinct().ToList();

            var authors = await _userRepository.GetUsersByIdsAsync(authorIds);
            var tagsInResponse = await _tagRepository.GetTagsByIdsAsync(tagIds);
            var authorDict = authors.ToDictionary(a => a.id, a => a.displayname);
            var tagDictForResponse = tagsInResponse.ToDictionary(t => t.id, t => t.name);

            // 4. Map ra response
            var response = novels.Select(novel => new NovelRecommendationResponse
            {
                NovelId = novel.id,
                Title = novel.title,
                Description = novel.description,
                AuthorId = novel.author_id,
                AuthorName = authorDict.GetValueOrDefault(novel.author_id),
                NovelImage = novel.novel_image,
                NovelBanner = novel.novel_banner,
                Tags = novel.tags.Select(tagId => new TagListResponse
                {
                    TagId = tagId,
                    Name = tagDictForResponse.GetValueOrDefault(tagId)
                }).ToList(),
                Status = novel.status,
                IsPublic = novel.is_public,
                AllowComment = novel.allow_comment,
                IsPaid = novel.is_paid,
                IsLock = novel.is_lock,
                Price = novel.price,
                TotalChapters = novel.total_chapters,
                TotalViews = novel.total_views,
                Followers = novel.followers,
                RatingAvg = novel.rating_avg,
                RatingCount = novel.rating_count,
                CreateAt = novel.created_at,
                UpdateAt = novel.updated_at,
                Slug = novel.slug,
                Similarity = scoreDict.GetValueOrDefault(novel.id)
            }).ToList();

            return new ApiResponse
            {
                Success = true,
                Data = response
            };
        }
    }
}
