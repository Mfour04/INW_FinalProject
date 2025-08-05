using Application.Features.Rating.Command;
using Application.Features.Rating.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/novels/{novelId}/ratings")]
    [ApiController]
    public class RatingsController : ControllerBase
    {
        private readonly IMediator _mediator;
     
        public RatingsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult> GetRatings([FromRoute] string novelId, [FromQuery] GetRatingByNovelId query)
        {
            query.NovelId = novelId;

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> GetRatingDetail(string id)
        {
            GetRatingById query = new()
            {
                RatingId = id,
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> CreateRating([FromRoute] string novelId, [FromBody] CreateRatingCommand command)
        {
            command.NovelId = novelId;

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult> UpdateRating(string id, [FromBody] UpdateRatingCommand command)
        {
            command.RatingId = id;

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeletePost(string id)
        {
            DeleteRatingCommand command = new()
            {
                RatingId = id,
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}
