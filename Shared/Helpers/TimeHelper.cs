using System.Globalization;

namespace Shared.Helpers
{
    public static class TimeHelper
    {
        private static readonly TimeZoneInfo VietNamTimeZone;

        static TimeHelper()
        {
            try
            {
                // Linux, Azure, Docker, MacOS
                VietNamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
            }
            catch (TimeZoneNotFoundException)
            {
                // Windows fallback
                VietNamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            }
        }


        /// <summary>
        /// Lấy thời gian hiện tại theo giờ Việt Nam (UTC+7)
        /// </summary>
        public static DateTime NowVN => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, VietNamTimeZone);

        /// <summary>
        /// Lấy Ticks theo giờ Việt Nam
        /// </summary>
        public static long NowTicks => NowVN.Ticks;

        /// <summary>
        /// Chuyển DateTime UTC thành giờ Việt Nam
        /// </summary>
        public static DateTime ToVN(DateTime utcTime) => TimeZoneInfo.ConvertTimeFromUtc(utcTime, VietNamTimeZone);

        /// <summary>
        /// Lấy giờ Việt Nam từ Ticks
        /// </summary>
        public static DateTime FromTicks(long ticks) => new DateTime(ticks).ToLocalTime();

        /// <summary>
        /// Ticks tại 00:00 hôm nay (giờ VN)
        /// </summary>
        public static long StartOfTodayTicksVN => NowVN.Date.Ticks;

        /// <summary>
        /// Ticks tại 23:59:59.9999999 hôm nay (giờ VN)
        /// </summary>
        public static long EndOfTodayTicksVN => NowVN.Date.AddDays(1).AddTicks(-1).Ticks;

        public static DateTimeOffset NowVNOffset => new DateTimeOffset(NowVN, VietNamTimeZone.GetUtcOffset(NowVN));

        public static DateTimeOffset AddMinutes(int minutes) => NowVNOffset.AddMinutes(minutes);

        public static long NowUnixTimeSeconds => NowVNOffset.ToUnixTimeMilliseconds();
        
        /// Ticks tại 00:00 thứ 2 đầu tuần hiện tại (giờ VN)
        public static long StartOfCurrentWeekTicksVN =>
            NowVN.Date.AddDays(-(int)(NowVN.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)NowVN.DayOfWeek - 1)).Ticks;

        /// Ticks tại 23:59:59.9999999 Chủ nhật tuần này (giờ VN)
        public static long EndOfCurrentWeekTicksVN =>
            new DateTime(StartOfCurrentWeekTicksVN).AddDays(7).AddTicks(-1).Ticks;
        public static List<DateTime> GetDaysFromStartOfWeekToTodayVN()
        {
            var today = NowVN.Date;
            var startOfWeek = today.AddDays(-(int)(today.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)today.DayOfWeek - 1));

            var days = new List<DateTime>();
            for (var d = startOfWeek; d <= today; d = d.AddDays(1))
                days.Add(d);

            return days;
        }

        public static string DayOfWeekVN(DayOfWeek day)
        {
            return day switch
            {
                DayOfWeek.Sunday => "Chủ nhật",
                DayOfWeek.Monday => "Thứ 2",
                DayOfWeek.Tuesday => "Thứ 3",
                DayOfWeek.Wednesday => "Thứ 4",
                DayOfWeek.Thursday => "Thứ 5",
                DayOfWeek.Friday => "Thứ 6",
                DayOfWeek.Saturday => "Thứ 7",
                _ => ""
            };
        }

        public static bool IsBanExpired(long? bannedUntilTicks) =>
            bannedUntilTicks.HasValue && NowTicks > bannedUntilTicks.Value;
    }
}
