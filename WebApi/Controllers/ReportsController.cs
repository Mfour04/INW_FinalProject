using Application.Features.Report.Command;
using Application.Features.Report.Queries;
using Domain.Entities.System;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/reports")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ReportsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllReports([FromQuery] GetReports query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetReportById(string id)
        {
            GetReportById query = new()
            {
                ReportId = id
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpPost()]
        [Authorize]
        public async Task<IActionResult> CreateReport([FromBody] CreateReportCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPut("{id}/moderate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Moderate(string id, [FromBody] ModerateReportCommand command)
        {
            command.ReportId = id;

            var result = await _mediator.Send(command);
            return Ok(result);
        }


        // [HttpPut("update")]
        // public async Task<IActionResult> UpdateReport([FromBody] UpdateReportCommand report)
        // {
        //     if (report == null || report.ReportIds == null || !report.ReportIds.Any())
        //     {
        //         return BadRequest("Invalid report data.");
        //     }

        //     var result = await _mediator.Send(report);
        //     if (result.Success)
        //     {
        //         return Ok(result);
        //     }
        //     return BadRequest(result.Message);
        // }

        // [HttpDelete("{id}")]
        // public async Task<IActionResult> DeleteReport(string id)
        // {
        //     if (string.IsNullOrEmpty(id))
        //     {
        //         return BadRequest("Report ID cannot be null or empty.");
        //     }

        //     var result = await _mediator.Send(new DeleteReportCommand { ReportId = id });
        //     if (result.Success)
        //     {
        //         return Ok(result);
        //     }
        //     return BadRequest(result.Message);
        // }
    }
}
