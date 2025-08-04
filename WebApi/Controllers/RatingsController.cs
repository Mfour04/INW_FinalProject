using Application.Features.Novel.Queries;
using Application.Features.Rating.Command;
using Application.Features.Rating.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Rating;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RatingsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private string currentUserId =>
          User.FindFirst(ClaimTypes.NameIdentifier)?.Value
           ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
          ?? throw new UnauthorizedAccessException("User ID not found in token");
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
        [HttpGet("novel/{novelId}")]
        public async Task<IActionResult> GetRatingsByNovelId(string novelId, [FromQuery] int page = 0, [FromQuery] int limit = 10)
        {
            try
            {
                var result = await _mediator.Send(new GetRatingByNovelId
                {
                    NovelId = novelId,
                    Page = page,
                    Limit = limit
                });
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> CreateRating([FromBody] CreateRatingCommand command)
        {  
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPut("update")]
        [Authorize]
        public async Task<IActionResult> UpdateRating([FromBody] UpdateRatingCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
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
