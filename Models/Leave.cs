using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace attendance.Models
{
    /// <summary>
    /// Leave model - stores leave applications
    /// </summary>
    public enum LeaveType
    {
        Sick = 1,
        Casual = 2,
        Annual = 3,
        Emergency = 4,
        Other = 5
    }

    public enum LeaveStatus
    {
        Pending = 1,
        Approved = 2,
        Declined = 3
    }

    public class Leave
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public LeaveType Type { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Reason { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "date")]
        public DateTime StartDate { get; set; }

        [Required]
        [Column(TypeName = "date")]
        public DateTime EndDate { get; set; }

        [Required]
        public LeaveStatus Status { get; set; } = LeaveStatus.Pending;

        [MaxLength(500)]
        public string? AdminRemarks { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation property
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}

