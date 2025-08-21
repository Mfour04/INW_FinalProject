using AutoMapper;
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
    public class GetReadingProcessById : IRequest<ApiResponse>
    {
        public string ReadingProcessId { get; set; }
    }

    public class GetReadingProcessByIdHandler : IRequestHandler<GetReadingProcessById, ApiResponse>
    {
        private readonly IReadingProcessRepository _readingProcessRepository;
        private readonly IMapper _mapper;
        private readonly INovelRepository _novelRepository;
        private readonly IUserRepository _userRepository;
        public GetReadingProcessByIdHandler(IReadingProcessRepository readingProcessRepository, IMapper mapper
            , INovelRepository novelRepository, IUserRepository userRepository)
        {
            _readingProcessRepository = readingProcessRepository;
            _mapper = mapper;
            _novelRepository = novelRepository;
            _userRepository = userRepository;
        }

        public async Task<ApiResponse> Handle(GetReadingProcessById request, CancellationToken cancellationToken)
        {
            var readingProcess = await _readingProcessRepository.GetByIdAsync(request.ReadingProcessId);
            if (readingProcess == null)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "Reading process not found."
                };
            }
            var novel = await _novelRepository.GetByNovelIdAsync(readingProcess.novel_id);
            if (novel == null)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "Novel not found for this reading process."
                };
            }

            // Lấy thông tin author
            var author = await _userRepository.GetById(novel.author_id);

            // Map vào response
            var readingProcessResponse = _mapper.Map<ReadingProcessResponse>((readingProcess, novel, author));

            // Map tags nếu cần
            readingProcessResponse.Tags = novel.tags?.Select(tagId => new TagListResponse
            {
                TagId = tagId,
                Name = "" // TODO: Lấy tên tag nếu có service Tag
            }).ToList() ?? new List<TagListResponse>();

            return new ApiResponse
            {
                Success = true,
                Message = "Reading process retrieved successfully.",
                Data = readingProcessResponse
            };
        }
    }
}
