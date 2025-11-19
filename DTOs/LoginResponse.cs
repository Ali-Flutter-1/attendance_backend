namespace attendance.DTOs
{
    /// <summary>
    /// DTO for login response
    /// </summary>
    public class LoginResponse
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Domain { get; set; }
        public string? Address { get; set; }
        public string? ProfilePicturePath { get; set; }
        public bool IsAdmin { get; set; }
    }
}

