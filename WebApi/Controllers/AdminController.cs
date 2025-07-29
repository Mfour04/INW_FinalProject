using Application.Features.Admin.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
    }
}
