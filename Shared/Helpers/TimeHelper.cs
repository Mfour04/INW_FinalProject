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
    }
}
