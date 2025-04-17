using MediatR;
using Shared.Contracts.Request;
using Shared.Contracts.Respone;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.Users
{
    public class LoginCommand : IRequest<ApiResponse>
    {
        public LoginRequest? LoginRequest { get; set; }
    }
}
