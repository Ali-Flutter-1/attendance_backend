using System;

namespace attendance.Services
{
    /// <summary>
    /// Service for handling Karachi, Pakistan timezone (PKT - UTC+5)
    /// </summary>
    public class TimeZoneService
    {
        private static TimeZoneInfo? _karachiTimeZone;

        private static TimeZoneInfo KarachiTimeZone
        {
            get
            {
                if (_karachiTimeZone == null)
                {
                    try
                    {
                        // Try Windows timezone ID first
                        _karachiTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time");
                    }
                    catch
                    {
                        try
                        {
                            // Try Linux/Unix timezone ID
                            _karachiTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Karachi");
                        }
                        catch
                        {
                            // Fallback: Create a custom timezone for UTC+5 (Pakistan Standard Time)
                            _karachiTimeZone = TimeZoneInfo.CreateCustomTimeZone(
                                "Pakistan Standard Time",
                                TimeSpan.FromHours(5),
                                "Pakistan Standard Time",
                                "Pakistan Standard Time");
                        }
                    }
                }
                return _karachiTimeZone;
            }
        }

        /// <summary>
        /// Get current time in Karachi timezone
        /// </summary>
        public static DateTime GetKarachiTime()
        {
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, KarachiTimeZone);
        }

        /// <summary>
        /// Convert UTC time to Karachi time
        /// </summary>
        public static DateTime ConvertToKarachiTime(DateTime utcTime)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(utcTime, KarachiTimeZone);
        }

        /// <summary>
        /// Convert Karachi time to UTC
        /// </summary>
        public static DateTime ConvertToUtc(DateTime karachiTime)
        {
            return TimeZoneInfo.ConvertTimeToUtc(karachiTime, KarachiTimeZone);
        }

        /// <summary>
        /// Get today's date in Karachi timezone
        /// </summary>
        public static DateTime GetKarachiDate()
        {
            return GetKarachiTime().Date;
        }

        /// <summary>
        /// Get current year in Karachi timezone
        /// </summary>
        public static int GetKarachiYear()
        {
            return GetKarachiTime().Year;
        }

        /// <summary>
        /// Get current month in Karachi timezone
        /// </summary>
        public static int GetKarachiMonth()
        {
            return GetKarachiTime().Month;
        }
    }
}

