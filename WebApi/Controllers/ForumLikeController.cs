using Application.Features.Forum.Commands;
using Application.Features.Forum.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/forums/likes")]
    public class ForumLikeController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ForumLikeController(IMediator mediator)
        {
            _mediator = mediator;
        }

      
    }
}