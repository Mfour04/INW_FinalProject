using System.Security.Claims;
using Application.Features.Transaction.Commands;
using Application.Features.Transaction.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/transactions")]
    public class TransactionController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly string _baseUrl;

        private string currentUserId =>
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("User ID không tìm thấy trong token");

        public TransactionController(IMediator mediator, IConfiguration config)
        {
            _mediator = mediator;
            _baseUrl = config["FeUrl"];
        }

        [HttpGet()]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetTransactions([FromQuery] GetTransactions query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("withdraws/pending")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPendingWithdraws([FromQuery] GetPendingWithdraws query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("user")]
        [Authorize]
        public async Task<IActionResult> GetUserTransaction([FromQuery] GetUserTransaction query)
        {
            query.UserId = currentUserId;

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetTransactionDetail(string id)
        {
            GetTransactionById query = new()
            {
                TransactionId = id,
                CurrentUserId = currentUserId,
                IsAdmin = User.IsInRole("Admin")
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpPost("recharges")]
        [Authorize]
        public async Task<IActionResult> CreateCoinRecharge([FromBody] CreateCoinRechargeCommand command)
        {
            command.UserId = currentUserId;

            var url = await _mediator.Send(command);
            return Ok(new { checkoutUrl = url });
        }

        [HttpPost("withdraws")]
        [Authorize]
        public async Task<IActionResult> RequestWithdraw([FromBody] WithdrawRequestCommand command)
        {
            command.UserId = currentUserId;

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPut("withdraws/{id}/cancel")]
        [Authorize]
        public async Task<IActionResult> CancelWithdraw(string id, [FromBody] CancelWithdrawRequestCommand command)
        {
            command.TransactionId = id;
            command.UserId = currentUserId;

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPut("withdraws/{id}/process")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ProcessWithdraw(string id, [FromBody] ProcessWithdrawRequestCommand command)
        {
            command.TransactionId = id;
            command.ApproverId = currentUserId;

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpGet("dashboard/chart")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetDashboardChart([FromQuery] GetTransactionDashboardChart query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("dashboard/summary")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetDashboardSummary([FromQuery] GetTransactionDashboardSummary query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        // ===== CALLBACKS PayOS =====

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
                return Redirect($"{_baseUrl}");
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
                return Redirect($"{_baseUrl}");
            }

            return BadRequest();
        }
    }
}