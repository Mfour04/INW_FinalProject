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
    public class RegisterUserCommand: IRequest<ApiResponse>
    {
        public RegisterRequest? RegisterRequest { get; set; }
    }
}
