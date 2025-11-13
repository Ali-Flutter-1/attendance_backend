using System.ComponentModel.DataAnnotations;

namespace attendance.DTOs
{
    /// <summary>
    /// DTO for leave application request
    /// </summary>
    public class LeaveRequest
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int Type { get; set; } // 1=Sick, 2=Casual, 3=Annual, 4=Emergency, 5=Other

        [Required]
        [MaxLength(1000)]
        public string Reason { get; set; } = string.Empty;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }
    }
}

