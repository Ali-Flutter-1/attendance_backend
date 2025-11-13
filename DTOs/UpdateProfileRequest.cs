using System.ComponentModel.DataAnnotations;

namespace attendance.DTOs
{
    /// <summary>
    /// DTO for updating user profile
    /// </summary>
    public class UpdateProfileRequest
    {
        [Required]
        public int UserId { get; set; }

        [MaxLength(50)]
        public string? FirstName { get; set; }

        [MaxLength(50)]
        public string? LastName { get; set; }

        [EmailAddress]
        [MaxLength(100)]
        public string? Email { get; set; }

        [MaxLength(100)]
        public string? Domain { get; set; }

        // Profile picture will be sent as multipart/form-data
    }
}

