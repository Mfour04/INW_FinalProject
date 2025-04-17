using MediatR;
using Shared.Contracts.Respone;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Queries
{
    public class GetUserByIdQuery : IRequest<ApiResponse>
    {
        public string UserId { get; set; }
    }
}
