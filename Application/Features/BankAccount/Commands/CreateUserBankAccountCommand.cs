using Application.Services.Interfaces;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.UserBankAccount;
using Shared.Helpers;

namespace Application.Features.BankAccount.Commands
{
    public class CreateUserBankAccountCommand : IRequest<ApiResponse>
    {
        public int BankBin { get; set; }
        public string BankCode { get; set; }
        public string BankShortName { get; set; }
        public string BankAccountNumber { get; set; }
        public string BankAccountName { get; set; }
        public bool IsDefault { get; set; }
    }

    public class CreateUserBankAccountCommandHandler : IRequestHandler<CreateUserBankAccountCommand, ApiResponse>
    {
        private readonly IUserBankAccountRepository _bankRepo;
        private readonly ICurrentUserService _currentUser;
        private readonly IMapper _mapper;

        public CreateUserBankAccountCommandHandler(IUserBankAccountRepository bankRepo, ICurrentUserService currentUser, IMapper mapper)
        {
            _bankRepo = bankRepo;
            _currentUser = currentUser;
            _mapper = mapper;
        }

        public async Task<ApiResponse> Handle(CreateUserBankAccountCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId;
            if (string.IsNullOrWhiteSpace(userId))
                return new ApiResponse { Success = false, Message = "User not authenticated." };

            if (request.BankBin <= 0)
                return new ApiResponse { Success = false, Message = "Bank bin is invalid." };

            if (!request.BankAccountNumber.All(char.IsDigit))
                return new ApiResponse { Success = false, Message = "Bank account number must be numeric." };

            if (request.BankAccountNumber.Length < 6 || request.BankAccountNumber.Length > 20)
                return new ApiResponse { Success = false, Message = "Bank account number length must be between 6 and 20 digits." };

            UserBankAccountEntity entity = new()
            {
                id = SystemHelper.RandomId(),
                user_id = userId,
                bank_bin = request.BankBin,
                bank_code = request.BankCode,
                bank_short_name = request.BankShortName,
                bank_account_number = request.BankAccountNumber,
                bank_account_name = request.BankAccountName,
                is_default = request.IsDefault,
                created_at = TimeHelper.NowTicks
            };

            await _bankRepo.AddAsync(entity);

            if (request.IsDefault)
            {
                await _bankRepo.SetDefaultAsync(userId, entity.id);
            }

            var response = _mapper.Map<BankAccountResponse>(entity);

            return new ApiResponse
            {
                Success = true,
                Message = "Bank account created successfully.",
                Data = response
            };
        }
    }
}