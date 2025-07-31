using Domain.Entities;
using Infrastructure.Repositories.Implements;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Novel;
using Shared.Contracts.Response.Tag;
using Shared.Helpers;

namespace Application.Features.Novel.Queries
{
    public class GetRecommendedNovels: IRequest<ApiResponse>
    {
        public string UserId { get; set; }
        public int TopN { get; set; } = 10;
    }
    public class GetRecommendedNovelsHandler : IRequestHandler<GetRecommendedNovels, ApiResponse>
    {
        private readonly IOpenAIRepository _openAIRepository;
        private readonly INovelRepository _novelRepository;
        private readonly IUserRepository _userRepository;
        private readonly ITagRepository _tagRepository;

        public GetRecommendedNovelsHandler(IOpenAIRepository openAIRepository, INovelRepository novelRepository,
            IUserRepository userRepository, ITagRepository tagRepository)
        {
            _openAIRepository = openAIRepository;
            _novelRepository = novelRepository;
            _userRepository = userRepository;
            _tagRepository = tagRepository;
        }

        public async Task<ApiResponse> Handle(GetRecommendedNovels request, CancellationToken cancellationToken)
        {
            // 1. Lấy embedding của user
            var userEmbedding = await _openAIRepository.GetUserEmbeddingAsync(request.UserId);
            if (userEmbedding == null)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "User embedding not found."
                };
            }

            // 2. Lấy thông tin user
            var user = await _userRepository.GetById(request.UserId);
            if (user == null)
            {
                return new ApiResponse { Success = false, Message = "User not found." };
            }

            var tagEntities = await _tagRepository.GetTagsByIdsAsync(user.favourite_type);

            var userTags = tagEntities
                .Select(t => t.name?.Trim().ToLowerInvariant())
                .Where(name => !string.IsNullOrEmpty(name))
                .ToHashSet();

            if (!userTags.Any())
            {
                return new ApiResponse { Success = false, Message = "User has no favorite tags." };
            }

            // 4. Lấy tất cả novel embeddings
            var novelEmbeddings = await _openAIRepository.GetAllNovelEmbeddingsAsync();
            if (novelEmbeddings == null || !novelEmbeddings.Any())
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "No novel embeddings found."
                };
            }

            // 5. Lấy toàn bộ tag để ánh xạ id -> name
            var allTagIds = novelEmbeddings.SelectMany(n => n.tags).Distinct().ToList();
            var allTags = await _tagRepository.GetTagsByIdsAsync(allTagIds);
            var tagDict = allTags.ToDictionary(t => t.id, t => t.name.Trim().ToLowerInvariant());

            // 6. Lọc truyện có tag name trùng với user
            var filteredNovelEmbeddings = novelEmbeddings
                .Where(novel =>
                    novel.vector_novel != null &&
                    novel.vector_novel.Count == userEmbedding.vector_user.Count 
                )
                .Select(novel => new
                {
                    novel.novel_id,
                    similarity = SystemHelper.CalculateCosineSimilarity(userEmbedding.vector_user, novel.vector_novel)
                })
                .Where(x => x.similarity >= 0.5)
                .OrderByDescending(x => x.similarity)
                .Take(request.TopN)
                .ToList();

            if (!filteredNovelEmbeddings.Any())
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "No similar novels found."
                };
            }

            // 7. Lấy chi tiết truyện
            var topNovelIds = filteredNovelEmbeddings.Select(n => n.novel_id).ToList();
            var novels = await _novelRepository.GetManyByIdsAsync(topNovelIds);
            var authorIds = novels.Select(n => n.author_id).Distinct().ToList();
            var novelTagIds = novels.SelectMany(n => n.tags).Distinct().ToList();

            var authors = await _userRepository.GetUsersByIdsAsync(authorIds);
            var tagsInResponse = await _tagRepository.GetTagsByIdsAsync(novelTagIds);

            var scoreDict = filteredNovelEmbeddings.ToDictionary(x => x.novel_id, x => x.similarity);
            var authorDict = authors.ToDictionary(a => a.id, a => a.displayname);
            var tagDictForResponse = tagsInResponse.ToDictionary(t => t.id, t => t.name);

            // 8. Tạo response
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
                Message = "Recommendation successful.",
                Data = new
                {
                    TotalNovels = response.Count,
                    Novels = response
                }
            };
        }
    }

}
