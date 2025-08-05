using Application.Features.BankAccount.Commands;
using Application.Features.BankAccount.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/banks")]
    public class UserBankAccountController : Controller
    {
        private readonly IMediator _mediator;

        public UserBankAccountController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult> GetSupportedBanks()
        {
            var result = await _mediator.Send(new GetSupportedBanks());
            return Ok(result);
        }

        [HttpGet("user")]
        [Authorize]
        public async Task<ActionResult> GetUserBankAccounts([FromQuery] string? username)
        {
            GetUserBankAccounts query = new()
            {
                UserName = username
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpPost("user")]
        [Authorize(Roles = "User")]
        public async Task<ActionResult> CreateUserBankAccount([FromBody] CreateUserBankAccountCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPost("qr")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GenerateWithdrawQr([FromBody] GenerateWithdrawQrCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPut("user/{id}/default")]
        public async Task<ActionResult> SetDefaultBankAccount(string id)
        {
            SetDefaultBankAccountCommand command = new()
            {
                Id = id
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpDelete("user/{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteUserBankAccount(string id)
        {
            DeleteUserBankAccountCommand command = new()
            {
                Id = id
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}