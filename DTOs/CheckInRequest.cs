using System.ComponentModel.DataAnnotations;

namespace attendance.DTOs
{
    /// <summary>
    /// DTO for check-in request
    /// </summary>
    public class CheckInRequest
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }

        // Picture will be sent as multipart/form-data
    }
}

