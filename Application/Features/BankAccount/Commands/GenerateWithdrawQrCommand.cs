using Application.Services.Interfaces;
using MediatR;
using Shared.Contracts.Response;

namespace Application.Features.BankAccount.Commands
{
    public class GenerateWithdrawQrCommand : IRequest<ApiResponse>
    {
        public long AccountNo { get; set; }
        public string AccountName { get; set; }
        public int AcqId { get; set; }
        public decimal Amount { get; set; }
        public string AddInfo { get; set; }
    }

    public class GenerateWithdrawQrCommandHandler : IRequestHandler<GenerateWithdrawQrCommand, ApiResponse>
    {
        private readonly IVietQrService _vietQrService;

        public GenerateWithdrawQrCommandHandler(IVietQrService vietQrService)
        {
            _vietQrService = vietQrService;
        }

        public async Task<ApiResponse> Handle(GenerateWithdrawQrCommand command, CancellationToken cancellationToken)
        {
            if (command.AccountNo <= 0)
                return new ApiResponse { Success = false, Message = "Invalid account number." };

            if (string.IsNullOrWhiteSpace(command.AccountName))
                return new ApiResponse { Success = false, Message = "Account name cannot be empty." };

            if (command.AcqId <= 0)
                return new ApiResponse { Success = false, Message = "Invalid bank code (AcqId)." };

            if (command.Amount <= 0)
                return new ApiResponse { Success = false, Message = "Amount must be greater than 0." };

            if (string.IsNullOrWhiteSpace(command.AddInfo))
                return new ApiResponse { Success = false, Message = "Transfer description cannot be empty." };

            var qrImageUrl = await _vietQrService.GenerateWithdrawQrAsync(
                command.AccountNo,
                command.AccountName,
                command.AcqId,
                command.Amount,
                command.AddInfo
            );

            return new ApiResponse
            {
                Success = true,
                Message = "QR code generated successfully.",
                Data = qrImageUrl
            };
        }
    }
}