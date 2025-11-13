namespace attendance.DTOs
{
    /// <summary>
    /// DTO for monthly attendance statistics
    /// </summary>
    public class MonthlyAttendanceResponse
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int Year { get; set; }
        public int Month { get; set; }
        public int TotalPresent { get; set; }
        public int TotalAbsent { get; set; }
        public double PresentPercentage { get; set; }
        public double AbsentPercentage { get; set; }
        public List<AttendanceResponse> DailyAttendances { get; set; } = new List<AttendanceResponse>();
    }
}

