using Application.Features.Report.Command;
using Application.Features.Report.Queries;
using Domain.Entities.System;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly FindCreterias _findCreterias;
        public ReportsController(IMediator mediator)
        {
            _mediator = mediator;
            _findCreterias = new FindCreterias();
        }
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 0,
            [FromQuery] int limit = 10,
            [FromQuery] string userId = null,
            [FromQuery] string novelId = null,
            [FromQuery] string chapterId = null,
            [FromQuery] string commentId = null,
            [FromQuery] string forumPostId = null,
            [FromQuery] string forumCommentId = null,
            [FromQuery] ReportTypeStatus? type = null,
            [FromQuery] ReportStatus? status = null)
        {
            var query = new GetReports
            {
                Page = page,
                Limit = limit,
                UserId = userId,
                NovelId = novelId,
                ChapterId = chapterId,
                CommentId = commentId,
                ForumPostId = forumPostId,
                ForumCommentId = forumCommentId,
                Type = type,
                Status = status
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }
        [HttpPost("create")]
        public async Task<IActionResult> CreateReport([FromBody] CreateReportCommand report)
        {
            if (report == null)
            {
                return BadRequest("Invalid report data.");
            }

            var result = await _mediator.Send(report);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result.Message);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetReportById(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("Report ID cannot be null or empty.");
            }

            var report = await _mediator.Send(new GetReportById { ReportId = id });
            if (report == null)
            {
                return NotFound("Report not found.");
            }

            return Ok(report);
        }
        [HttpPut("update")]
        public async Task<IActionResult> UpdateReport([FromBody] UpdateReportCommand report)
        {
            if (report == null || report.ReportIds == null || !report.ReportIds.Any())
            {
                return BadRequest("Invalid report data.");
            }

            var result = await _mediator.Send(report);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result.Message);
        }   
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReport(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("Report ID cannot be null or empty.");
            }

            var result = await _mediator.Send(new DeleteReportCommand { ReportId = id });
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result.Message);
        }
    }
}
