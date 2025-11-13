namespace attendance.DTOs
{
    /// <summary>
    /// DTO for attendance response
    /// </summary>
    public class AttendanceResponse
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public string? CheckInPicturePath { get; set; }
        public string? CheckOutPicturePath { get; set; }
        public bool IsPresent { get; set; }
        public bool IsAbsent { get; set; }
        public bool IsLateCheckIn { get; set; }
        public bool IsEarlyCheckOut { get; set; }
    }
}

