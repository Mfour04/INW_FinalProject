using AutoMapper;
using DnsClient;
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
    public class GetNovelByAuthorId: IRequest<ApiResponse>
    {
        public string AuthorId { get; set; }
    }

    public class GetNovelByAuthorIdHandler : IRequestHandler<GetNovelByAuthorId, ApiResponse>
    {
        private readonly INovelRepository _novelRepository;
        private readonly ITagRepository _tagRepository;
        private readonly IMapper _mapper;
        private readonly IUserRepository _userRepository;
        public GetNovelByAuthorIdHandler(INovelRepository novelRepository, IMapper mapper, ITagRepository tagRepository, IUserRepository userRepository)
        {
            _novelRepository = novelRepository;
            _mapper = mapper;
            _tagRepository = tagRepository;
            _userRepository = userRepository;
        }
        public async Task<ApiResponse> Handle(GetNovelByAuthorId request, CancellationToken cancellationToken)
        {
            var novelAuthor = await _novelRepository.GetNovelByAuthorId(request.AuthorId);
            var novelResponse = _mapper.Map<List<NovelResponse>>(novelAuthor);
            if (novelAuthor == null)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "AuthorId not found"
                };
            }

            var authorIds = novelAuthor.Select(n => n.author_id).Distinct().ToList();
            var authors = await _userRepository.GetUsersByIdsAsync(authorIds);
            var allTagIds = novelAuthor.SelectMany(n => n.tags).Distinct().ToList();
            var allTags = await _tagRepository.GetTagsByIdsAsync(allTagIds);

            for (int i = 0; i < novelAuthor.Count; i++)
            {
                var author = authors.FirstOrDefault(a => a.id == novelAuthor[i].author_id);
                if (author != null)
                {
                    novelResponse[i].AuthorName = author.displayname; // hoặc author.FullName, tùy DB bạn lưu
                }

                var tags = novelAuthor[i].tags;
                novelResponse[i].Tags = allTags
                    .Where(t => tags.Contains(t.id))
                    .Select(t => new TagListResponse
                    {
                        TagId = t.id,
                        Name = t.name
                    }).ToList();
            }

            var totalNovelViews = novelResponse.Sum(n => n.TotalViews);
            var totalComments = novelResponse.Sum(n => n.CommentCount);

            return new ApiResponse
            {
                Success = true,
                Message = "Get Novel By Novel By AuthorId Successfully",
                Data = new
                {
                    Novels = novelResponse,
                    TotalNovelViews = totalNovelViews,
                    TotalComments = totalComments
                }
            };
        }
    }
}
