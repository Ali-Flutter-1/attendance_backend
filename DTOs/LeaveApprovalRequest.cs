using System.ComponentModel.DataAnnotations;

namespace attendance.DTOs
{
    /// <summary>
    /// DTO for leave approval/decline request
    /// </summary>
    public class LeaveApprovalRequest
    {
        [Required]
        public int LeaveId { get; set; }

        [Required]
        public int Status { get; set; } // 2=Approved, 3=Declined

        [MaxLength(500)]
        public string? AdminRemarks { get; set; }
    }
}

