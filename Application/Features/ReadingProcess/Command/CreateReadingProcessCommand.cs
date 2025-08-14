using AutoMapper;
using Domain.Entities;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.ReadingProcess;
using Shared.Helpers;
using System.Text.Json.Serialization;

namespace Application.Features.ReadingProcess.Command
{
    public class CreateReadingProcessCommand : IRequest<ApiResponse>
    {
        public string UserId { get; set; }
        public string NovelId { get; set; }
        public string ChapterId { get; set; }
    }

    public class CreateReadingProcessCommandHandler : IRequestHandler<CreateReadingProcessCommand, ApiResponse>
    {
        private readonly IReadingProcessRepository _readingProcessRepository;
        private readonly IUserRepository _userRepository;
        private readonly INovelRepository _novelRepository;
        private readonly IChapterRepository _chapterRepository;
        private readonly IMapper _mapper;

        public CreateReadingProcessCommandHandler(IReadingProcessRepository readingProcessRepository, IUserRepository userRepository, INovelRepository novelRepository, IChapterRepository chapterRepository, IMapper mapper)
        {
            _readingProcessRepository = readingProcessRepository;
            _userRepository = userRepository;
            _novelRepository = novelRepository;
            _chapterRepository = chapterRepository;
            _mapper = mapper;
        }

        public async Task<ApiResponse> Handle(CreateReadingProcessCommand request, CancellationToken cancellationToken)
        {
            if (request.UserId != null)
            {
                var user = await _userRepository.GetById(request.UserId);
                if (user == null)
                {
                    return new ApiResponse
                    {
                        Success = false,
                        Message = "User not found."
                    };
                }
            }
            if (request.NovelId != null)
            {
                var novel = await _novelRepository.GetByNovelIdAsync(request.NovelId);
                if (novel == null)
                {
                    return new ApiResponse
                    {
                        Success = false,
                        Message = "Novel not found."
                    };
                }
            }
            if (request.ChapterId != null)
            {
                var chapter = await _chapterRepository.GetByIdAsync(request.ChapterId);
                if (chapter == null)
                {
                    return new ApiResponse
                    {
                        Success = false,
                        Message = "Chapter not found."
                    };
                }
            }

            var existingReadingProcess = await _readingProcessRepository.GetByUserAndNovelAsync(request.UserId, request.NovelId);
            if (existingReadingProcess != null)
            {
                existingReadingProcess.chapter_id = request.ChapterId;
                existingReadingProcess.updated_at = TimeHelper.NowTicks;

                var updatedReadingProcess = await _readingProcessRepository.UpdateAsync(existingReadingProcess);
                var updatedResponse = _mapper.Map<ReadingProcessResponse>(updatedReadingProcess);
                return new ApiResponse
                {
                    Success = true,
                    Message = "Reading process updated successfully.",
                    Data = updatedResponse
                };
            }
            else
            {
                var newReadingProcess = new ReadingProcessEntity
                {
                    id = SystemHelper.RandomId(),
                    user_id = request.UserId,
                    novel_id = request.NovelId,
                    chapter_id = request.ChapterId,
                    created_at = TimeHelper.NowTicks,
                    updated_at = TimeHelper.NowTicks
                };
                await _readingProcessRepository.CreateAsync(newReadingProcess);
                var createResponse = _mapper.Map<ReadingProcessResponse>(newReadingProcess);
                return new ApiResponse
                {
                    Success = true,
                    Message = "Reading process created successfully.",
                    Data = createResponse
                };
            }
        }
    }
}
