using Application.Features.Novel.Commands;
using Application.Features.NovelFollower.Commands;
using Application.Features.NovelFollower.Queries;
using Application.Features.Tag.Command;
using Application.Features.Tag.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NovelFollowersController : ControllerBase
    {
        private readonly IMediator _mediator;
        public NovelFollowersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("created")]
        public async Task<IActionResult> CreateTag([FromBody] CreateNovelFollowerCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTag(string id)
        {
            var result = await _mediator.Send(new DeleteNovelFollowerCommand { NovelFollowerId = id });
            return Ok(result);
        }

        [HttpGet("{novelId}")]
        public async Task<IActionResult> GetByNovelId(string novelId)
        {
            var result = await _mediator.Send(new GetFollowerByNovelIdCommand { NovelId = novelId });
            return Ok(result);
        }
    }
}
