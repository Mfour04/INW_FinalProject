using AutoMapper;
using Domain.Entities;
using Infrastructure.Repositories.Implements;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using MongoDB.Driver;
using Shared.Contracts.Response;
using Shared.Contracts.Response.ReadingProcess;
using Shared.Contracts.Response.Tag;
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
        private readonly ITagRepository _tagRepository;

        public CreateReadingProcessCommandHandler(IReadingProcessRepository readingProcessRepository, IUserRepository userRepository
            , INovelRepository novelRepository, IChapterRepository chapterRepository, IMapper mapper
            , ITagRepository tagRepository)
        {
            _readingProcessRepository = readingProcessRepository;
            _userRepository = userRepository;
            _novelRepository = novelRepository;
            _chapterRepository = chapterRepository;
            _mapper = mapper;
            _tagRepository = tagRepository;
        }

        public async Task<ApiResponse> Handle(CreateReadingProcessCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetById(request.UserId);
            if (user == null)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "Không tìm thấy người dùng."
                };
            }

            var novel = await _novelRepository.GetByNovelIdAsync(request.NovelId);
            if (novel == null)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "Không tìm thấy tiểu thuyết."
                };
            }

            var chapter = await _chapterRepository.GetByIdAsync(request.ChapterId);
            if (chapter == null)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "Chương không tìm thấy."
                };
            }

            var allTagIds = novel.tags.Distinct().ToList();
            var allTags = await _tagRepository.GetTagsByIdsAsync(allTagIds);
            var tagDict = allTags.ToDictionary(t => t.id, t => t.name);
            if (allTagIds.Count == 0)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "Tiểu thuyết không có thẻ hợp lệ."
                };
            }

            var existingReadingProcess = await _readingProcessRepository.GetByUserAndNovelAsync(request.UserId, request.NovelId);
            if (existingReadingProcess != null)
            {
                existingReadingProcess.chapter_id = request.ChapterId;
                existingReadingProcess.updated_at = TimeHelper.NowTicks;

                var updatedReadingProcess = await _readingProcessRepository.UpdateAsync(existingReadingProcess);

                // Fix: Use the correct overload of the Map method
                var updatedResponse = _mapper.Map<ReadingProcessResponse>((updatedReadingProcess, novel, user));
                updatedResponse.Tags = allTags
                    .Where(t => tagDict.ContainsKey(t.id))
                    .Select(t => new TagListResponse
                    {
                        TagId = t.id,
                        Name = t.name
                    }).ToList();

                return new ApiResponse
                {
                    Success = true,
                    Message = "Reading process đã được cập nhật thành công.",
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

                var createResponse = _mapper.Map<ReadingProcessResponse>((newReadingProcess, novel, user));
                createResponse.Tags = allTags
                    .Where(t => tagDict.ContainsKey(t.id))
                    .Select(t => new TagListResponse
                    {
                        TagId = t.id,
                        Name = t.name
                    }).ToList();

                return new ApiResponse
                {
                    Success = true,
                    Message = "Reading process đã tạo thành công.",
                    Data = createResponse
                };
            }
        }
    }
}
