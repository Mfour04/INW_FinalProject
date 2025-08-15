using AutoMapper;
using Domain.Entities;
using Domain.Entities.System;
using Infrastructure.Repositories.Implements;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.ReadingProcess;
using Shared.Contracts.Response.Tag;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.ReadingProcess.Queries
{
    public class GetReadingHistory : IRequest<ApiResponse>
    {
        public int Page = 0;
        public int Limit = 10;
        public string? UserId { get; set; }
    }

    public class GetReadingHistoryHandler : IRequestHandler<GetReadingHistory, ApiResponse>
    {
        private readonly IReadingProcessRepository _readingProcessRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly INovelRepository _novelRepository;
        public GetReadingHistoryHandler(
            IReadingProcessRepository readingProcessRepository,
            IUserRepository userRepository,
            IMapper mapper,
            INovelRepository novelRepository)
        {
            _readingProcessRepository = readingProcessRepository;
            _userRepository = userRepository;
            _mapper = mapper;
            _novelRepository = novelRepository;
        }

        public async Task<ApiResponse> Handle(GetReadingHistory request, CancellationToken cancellationToken)
        {
            FindCreterias findCreterias = new FindCreterias
            {
                Limit = request.Limit,
                Page = request.Page
            };
            List<ReadingProcessEntity> readingProcesses;

            if (string.IsNullOrEmpty(request.UserId))
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "UserId is required"
                };
            }
            readingProcesses = await _readingProcessRepository.GetReadingHistoryAsync(findCreterias, request.UserId);
            if (readingProcesses == null || !readingProcesses.Any())
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "No reading history found for this user"
                };
            }
            var novelIds = readingProcesses.Select(r => r.novel_id).Distinct().ToList();
            var novels = await _novelRepository.GetManyByIdsAsync(novelIds);

            // Lấy danh sách author_id từ novels
            var authorIds = novels.Select(n => n.author_id).Distinct().ToList();
            var authors = await _userRepository.GetUsersByIdsAsync(authorIds);

            // Map dữ liệu
            var response = new List<ReadingProcessResponse>();

            foreach (var process in readingProcesses)
            {
                var novel = novels.FirstOrDefault(n => n.id == process.novel_id);
                if (novel == null) continue;

                var author = authors.FirstOrDefault(a => a.id == novel.author_id);

                var mapped = _mapper.Map<ReadingProcessResponse>((process, novel, author));

                // Nếu cần map tags
                mapped.Tags = novel.tags?.Select(tagId => new TagListResponse
                {
                    TagId = tagId,
                    Name = "" // TODO: Lấy tên tag nếu có service Tag
                }).ToList() ?? new List<TagListResponse>();

                response.Add(mapped);
            }
            return new ApiResponse
            {
                Success = true,
                Message = "Reading history retrieved successfully",
                Data = response
            };
        }
    }
}
