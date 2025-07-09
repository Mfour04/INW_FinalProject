using System.Security.Claims;
using Application.Features.Transaction.Commands;
using Application.Features.Transaction.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts.Response;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/transactions")]
    public class TransactionController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TransactionController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet()]
        public async Task<IActionResult> GetTransactions([FromQuery] GetTransactions query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpPost("recharges")]
        public async Task<IActionResult> CreateCoinRecharge([FromBody] CreateCoinRechargeCommand command)
        {
            // command.UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            command.UserId = "user_002";
            var url = await _mediator.Send(command);
            return Ok(new { checkoutUrl = url });
        }

        [HttpGet("recharges/return-url")]
        public async Task<IActionResult> HandleReturn(
        [FromQuery] string code,
        [FromQuery] bool cancel,
        [FromQuery] string status,
        [FromQuery] long orderCode)
        {
            if (code == "00" && !cancel && status == "PAID")
            {
                await _mediator.Send(new ConfirmTransactionCommand
                {
                    OrderCode = orderCode
                });

                return Ok();
            }

            return BadRequest();
        }

        [HttpGet("recharges/cancel-url")]
        public async Task<IActionResult> HandleCancel(
        [FromQuery] bool cancel,
        [FromQuery] string status,
        [FromQuery] long orderCode)
        {
            if (cancel || status == "CANCELLED")
            {
                await _mediator.Send(new CancelTransactionCommand
                {
                    OrderCode = orderCode
                });

                return Ok();
            }

            return BadRequest();
        }

        // [Authorize]
        [HttpGet("user")]
        public async Task<IActionResult> GetUserTransaction([FromQuery] GetUserTransaction request)
        {
            // var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // if (string.IsNullOrEmpty(userId))
            //     return Unauthorized(new ApiResponse
            //     {
            //         Success = false,
            //         Message = "User not authenticated."
            //     });

            // request.UserId = userId;

            // request.UserId = "user_002";

            var result = await _mediator.Send(request);
            return Ok(result);
        }

        [HttpPost("withdraws")]
        public async Task<IActionResult> RequestWithdraw([FromBody] WithdrawRequestCommand command)
        {
            command.UserId = "user_002"; // hardcode tạm
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPost("withdraws/{id}/process")]
        public async Task<IActionResult> ProcessWithdraw(string id, [FromBody] ProcessWithdrawRequestCommand command)
        {
            command.TransactionId = id;
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPost("withdraws/{id}/cancel")]
        public async Task<IActionResult> CancelWithdraw(string id, [FromBody] CancelWithdrawRequestCommand command)
        {
            command.TransactionId = id;
            command.UserId = "user_002"; // hardcode tạm
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpGet("withdraws/pending")]
        public async Task<IActionResult> GetPendingWithdraws([FromQuery] GetPendingWithdraws query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }
    }
}