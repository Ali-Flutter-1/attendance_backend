using System.ComponentModel.DataAnnotations;

namespace attendance.DTOs
{
    /// <summary>
    /// DTO for user signup request
    /// Profile picture will be sent as multipart/form-data
    /// </summary>
    public class SignupRequest
    {
        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        [MaxLength(100)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string ConfirmPassword { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Domain { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }

        // Profile picture will be sent as multipart/form-data with field name "picture"
    }
}

