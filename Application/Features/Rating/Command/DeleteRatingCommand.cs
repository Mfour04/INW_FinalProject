using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Rating.Command
{
    public class DeleteRatingCommand : IRequest<ApiResponse>
    {
        public string RatingId { get; set; }
    }
    public class DeleteRatingCommandHandler : IRequestHandler<DeleteRatingCommand, ApiResponse>
    {
        private readonly IRatingRepository _ratingRepository;

        public DeleteRatingCommandHandler(IRatingRepository ratingRepository)
        {
            _ratingRepository = ratingRepository;
        }

        public async Task<ApiResponse> Handle(DeleteRatingCommand request, CancellationToken cancellationToken)
        {
            var deleted = await _ratingRepository.DeleteAsync(request.RatingId);
            if (deleted == null)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "Không tìm thấy đánh giá hoặc đánh giá đã bị xóa."
                };
            }
            return new ApiResponse
            {
                Success = true,
                Message = "Xóa đánh giá thành công.",
                Data = deleted
            };
        }
    }
}
