using Application.Features.Admin.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts.Response.Admin;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AdminController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("admin-dashboard")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAdminDashboardStats()
        {
            var result = await _mediator.Send(new GetAdminDashboardStats());
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpGet("analysis")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> GetTotalViews([FromQuery] GetAdminAnalysis request)
        {
            var result = await _mediator.Send(request);
            return Ok(result);
        }
    }
}
