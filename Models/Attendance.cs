using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace attendance.Models
{
    /// <summary>
    /// Attendance model - stores check-in and check-out records
    /// </summary>
    public class Attendance
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [Column(TypeName = "date")]
        public DateTime Date { get; set; }

        public DateTime? CheckInTime { get; set; }

        public DateTime? CheckOutTime { get; set; }

        [MaxLength(500)]
        public string? CheckInPicturePath { get; set; }

        [MaxLength(500)]
        public string? CheckOutPicturePath { get; set; }

        public double? CheckInLatitude { get; set; }

        public double? CheckInLongitude { get; set; }

        public double? CheckOutLatitude { get; set; }

        public double? CheckOutLongitude { get; set; }

        public bool IsPresent { get; set; } = false;

        public bool IsAbsent { get; set; } = false;

        public bool IsLateCheckIn { get; set; } = false;

        public bool IsEarlyCheckOut { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}

