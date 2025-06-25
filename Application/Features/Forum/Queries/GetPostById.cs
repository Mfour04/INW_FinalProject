using AutoMapper;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Forum;

namespace Application.Features.Forum.Queries
{
    public class GetPostById : IRequest<ApiResponse>
    {
        public string Id { get; set; }
    }

    public class GetPostByIdHanlder : IRequestHandler<GetPostById, ApiResponse>
    {
        private readonly IForumPostRepository _postRepo;
        private readonly IUserRepository _userRepo;
        private readonly IMapper _mapper;

        public GetPostByIdHanlder(IForumPostRepository postRepo, IUserRepository userRepo, IMapper mapper)
        {
            _postRepo = postRepo;
            _userRepo = userRepo;
            _mapper = mapper;
        }

        public async Task<ApiResponse> Handle(GetPostById request, CancellationToken cancellationToken)
        {

            var post = await _postRepo.GetByIdAsync(request.Id);

            if (post == null)
            {
                return new ApiResponse { Success = false, Message = "No forum posts found." };
            }

            var response = _mapper.Map<PostResponse>(post);

            var user = await _userRepo.GetById(post.user_id);
            if (user != null)
            {
                response.Author = new ForumPostAuthorResponse
                {
                    Id = user.id,
                    Username = user.username,
                    Avatar = user.avata_url
                };
            }
            
            return new ApiResponse { Success = true, Data = response };
        }
    }
}