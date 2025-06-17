using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/badges/progress")]
    public class BadgeProgressController  : ControllerBase
    {
        private readonly IMediator _mediator;

        public BadgeProgressController (IMediator mediator)
        {
            _mediator = mediator;
        }

        // [HttpGet("{userId}")]
        // [HttpGet("/{userId}/completed")]
        // [HttpPost("init/{userId}")]
        // [HttpPut("{userId}/visible")]
    }
}