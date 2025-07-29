using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Contracts.Response.Admin
{
    public class AdminDashboardStatsResponse
    {
        public int TotalUsers { get; set; }
        public int NewUsersToday { get; set; }
        public List<WeeklyStatItem> NewUsersPerDay { get; set; }

        public int TotalNovels { get; set; }
        public List<WeeklyStatItem> NewNovelsPerDay { get; set; }
    }
    public class WeeklyStatItem
    {
        public string Day { get; set; } // eg: "Thứ 2"
        public string Weekday { get; set; }
        public int Count { get; set; }

    }
}
