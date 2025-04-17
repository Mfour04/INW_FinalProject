using Application.Queries;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Respone;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Handlers.User
{
    public class GetUserByIdHanlder : IRequestHandler<GetUserByIdQuery, ApiResponse>
    {
        private readonly IUserRepository _userRepository;
        public GetUserByIdHanlder(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        public async Task<ApiResponse> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _userRepository.GetById(request.UserId);
                if (user == null)
                    return new ApiResponse { Success = false, Message = "User not found" };

                return new ApiResponse { Success = true, Data = user };
            } catch (Exception ex)
            {
                return new ApiResponse { Success = false, Message = $"An error occurred: {ex.Message}" };
            }
        }
    }
}
