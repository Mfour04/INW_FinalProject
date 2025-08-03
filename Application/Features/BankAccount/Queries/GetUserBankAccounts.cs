using Application.Services.Interfaces;
using AutoMapper;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.UserBankAccount;

namespace Application.Features.BankAccount.Queries
{
    public class GetUserBankAccounts : IRequest<ApiResponse>
    {
        public string? UserName { get; set; }
    }

    public class GetUserBankAccountsHandler : IRequestHandler<GetUserBankAccounts, ApiResponse>
    {
        private readonly IUserBankAccountRepository _bankRepo;
        private readonly IUserRepository _userRepo;
        private readonly ICurrentUserService _currentUser;
        private readonly IMapper _mapper;

        public GetUserBankAccountsHandler(IUserBankAccountRepository bankRepo, IUserRepository userRepo, ICurrentUserService currentUser, IMapper mapper)
        {
            _bankRepo = bankRepo;
            _userRepo = userRepo;
            _currentUser = currentUser;
            _mapper = mapper;
        }

        public async Task<ApiResponse> Handle(GetUserBankAccounts request, CancellationToken cancellationToken)
        {
            var currentUserId = _currentUser.UserId;
            string targetUserId = currentUserId;

            if (!string.IsNullOrEmpty(request.UserName))
            {
                if (!_currentUser.IsAdmin())
                {
                    return new ApiResponse
                    {
                        Success = false,
                        Message = "You are not authorized to view this user's bank accounts."
                    };
                }

                var user = await _userRepo.GetByName(request.UserName);
                if (user == null)
                {
                    return new ApiResponse
                    {
                        Success = false,
                        Message = "User not found."
                    };
                }

                targetUserId = user.id;
            }

            var accounts = await _bankRepo.GetByUserAsync(targetUserId);
            var response = _mapper.Map<List<BankAccountResponse>>(accounts);

            return new ApiResponse
            {
                Success = true,
                Message = "Bank accounts retrieved successfully.",
                Data = response
            };
        }
    }
}