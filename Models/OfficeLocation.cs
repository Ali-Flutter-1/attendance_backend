using System.ComponentModel.DataAnnotations;

namespace attendance.Models
{
    /// <summary>
    /// OfficeLocation model - stores office location coordinates
    /// </summary>
    public class OfficeLocation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = "Main Office";

        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }

        [Required]
        public double AllowedRadiusInMeters { get; set; } = 50.0; // Default 50 meters

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

