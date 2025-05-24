using Application.Features.User.Queries;
using Infrastructure.Repositories.Implements;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Respone;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Novel.Queries
{
    public class GetNovelById: IRequest<ApiResponse>
    {
        public string NovelId { get; set; }
    }
    public class GetNovelHanlder : IRequestHandler<GetNovelById, ApiResponse>
    {
        private readonly INovelRepository _novelRepository;
        public GetNovelHanlder(INovelRepository novelRepository)
        {
            _novelRepository = novelRepository;
        }
        public async Task<ApiResponse> Handle(GetNovelById request, CancellationToken cancellationToken)
        {
            try
            {
                var novel = await _novelRepository.GetByNovelIdAsync(request.NovelId);
                if (novel == null)
                    return new ApiResponse { Success = false, Message = "Novel not found" };

                return new ApiResponse { Success = true, Data = novel };
            }
            catch (Exception ex)
            {
                return new ApiResponse { Success = false, Message = $"An error occurred: {ex.Message}" };
            }
        }
    }
}
