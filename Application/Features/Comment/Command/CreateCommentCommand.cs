using AutoMapper;
using Domain.Entities;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Comment;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Application.Features.Comment.Command
{
    public class CreateCommentCommand : IRequest<ApiResponse>
    {
        [JsonPropertyName("comment")]
        public CreateCommentResponse Comment { get; set; }
    }

    public class CreateCommentCommandHandler : IRequestHandler<CreateCommentCommand, ApiResponse>
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IChapterRepository _chapterRepository;
        private readonly INovelRepository _novelRepository;
        private readonly IMapper _mapper;

        public CreateCommentCommandHandler(
            ICommentRepository commentRepository,
            IChapterRepository chapterRepository,
            INovelRepository novelRepository,
            IMapper mapper)
        {
            _commentRepository = commentRepository;
            _chapterRepository = chapterRepository;
            _novelRepository = novelRepository;
            _mapper = mapper;
        }

        public async Task<ApiResponse> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
        {
            var novel = await _novelRepository.GetByNovelIdAsync(request.Comment.NovelId);
            if (novel == null)
                return new ApiResponse { Success = false, Message = "Novel not found" };
            if (request.Comment.ChapterId != null)
            {
                var chapter = await _chapterRepository.GetByChapterIdAsync(request.Comment.ChapterId);
            }
            var createdComment = new CommentEntity
            {
                id = SystemHelper.RandomId(),
                novel_id = novel.id,
                chapter_id = request.Comment.ChapterId,
                user_id = request.Comment.UserId,
                content = request.Comment.Content,
                parent_comment_id = request.Comment.ParentCommentId,
                created_at = DateTime.UtcNow.Ticks,
                updated_at = DateTime.UtcNow.Ticks
            };
            await _commentRepository.CreateCommentAsync(createdComment);
            var response = _mapper.Map<CommentResponse>(createdComment);

            return new ApiResponse
            {
                Success = true,
                Message = "Comment created successfully",
                Data = response
            };
        }
    }
}
