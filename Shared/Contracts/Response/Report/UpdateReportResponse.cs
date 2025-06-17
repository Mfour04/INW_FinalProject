using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Contracts.Response.Report
{
    public class UpdateReportResponse
    {
        public string ReportId { get; set; }
        public ReportStatus Status { get; set; }
    }
}
