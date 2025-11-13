namespace attendance.DTOs
{
    /// <summary>
    /// DTO for leave response
    /// </summary>
    public class LeaveResponse
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? AdminRemarks { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

