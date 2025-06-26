using Application.Features.Rating.Command;
using Application.Features.Rating.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts.Response.Rating;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RatingsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public RatingsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetRatings([FromQuery] string novelId = null)
        {
            try
            {
                var query = new GetRatings { NovelId = novelId };
                var result = await _mediator.Send(query);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRatingById(string id)
        {
            try
            {
                var query = new GetRatingById { RatingId = id };
                var result = await _mediator.Send(query);

                if (result == null)
                    return NotFound("Rating not found");

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateRating([FromBody] CreateRatingCommand command)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                if (command == null)
                {
                    return BadRequest("Invalid rating data.");
                }
                var result = await _mediator.Send(command);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateRating([FromBody] UpdateRatingCommand command)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (command == null)
                {
                    return BadRequest("Invalid rating data.");
                }

                var result = await _mediator.Send(command);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRating(string id)
        {
            try
            {
                var command = new DeleteRatingCommand { RatingId = id };
                var result = await _mediator.Send(command);

                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result.Message);

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
