using Application.Features.Transaction.Commands;
using Application.Features.Transaction.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<IActionResult> GetTransactions([FromQuery] GetTransactions request)
        {
            var result = await _mediator.Send(request);
            return Ok(result);
        }

        [HttpPost("coin-recharge")]
        public async Task<IActionResult> CreateCoinRecharge([FromBody] CreateCoinRechargeCommand command)
        {
            // command.UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            command.UserId = "user_002";
            var url = await _mediator.Send(command);
            return Ok(new { checkoutUrl = url });
        }

        [HttpGet("return-url")]
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

        [HttpGet("cancel-url")]
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

        [HttpGet("user-transaction")]
        public async Task<IActionResult> GetUserTransaction([FromQuery] GetUserTransaction request)
        {
            // var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // if (string.IsNullOrEmpty(userId))
            //     return Unauthorized(new ApiResponse
            //     {
            //         Success = false,
            //         Message = "User not authenticated."
            //     });
            // command.UserId = userId;

            request.UserId = "user_002";

            var result = await _mediator.Send(request);
            return Ok(result);
        }
    }
}