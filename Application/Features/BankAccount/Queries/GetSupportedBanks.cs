using Application.Services.Interfaces;
using MediatR;
using Shared.Contracts.Response;

namespace Application.Features.BankAccount.Queries
{
    public class GetSupportedBanks : IRequest<ApiResponse> { }

    public class GetSupportedBanksHandler : IRequestHandler<GetSupportedBanks, ApiResponse>
    {
        private readonly IVietQrService _vietQrService;

        public GetSupportedBanksHandler(IVietQrService vietQrService)
        {
            _vietQrService = vietQrService;
        }

        public async Task<ApiResponse> Handle(GetSupportedBanks request, CancellationToken cancellationToken)
        {
            var banks = await _vietQrService.GetSupportedBanksAsync();

            return new ApiResponse
            {
                Success = true,
                Message = "Supported banks retrieved successfully.",
                Data = banks
            };
        }
    }
}