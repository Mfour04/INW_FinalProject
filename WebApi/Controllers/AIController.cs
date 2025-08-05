using Application.Features.OpenAI.Commands;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AIController : ControllerBase
    {
        private readonly IMediator _mediator;
        public AIController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("check")]
        public async Task<IActionResult> CheckModeration([FromBody] CheckModerationCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPost("plagiarism")]
        public async Task<IActionResult> CheckPlagiarism([FromBody] PlagiarismChapterConent command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}
