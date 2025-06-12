using AutoMapper;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;

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
            try
            {
                var post = await _postRepo.GetByIdAsync(request.Id);

                if (post == null)
                {
                    return new ApiResponse { Success = false, Message = "No forum posts found." };
                }

                return new ApiResponse { Success = true, Data = post };
            }
            catch (Exception ex)
            {
                return new ApiResponse { Success = false, Message = $"An error occurred: {ex.Message}" };
            }
        }
    }
}