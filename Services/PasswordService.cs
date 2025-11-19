using System.Security.Cryptography;
using System.Text;

namespace attendance.Services
{
    /// <summary>
    /// Service for password hashing and verification
    /// Uses SHA256 hashing algorithm
    /// </summary>
    public class PasswordService
    {
        /// <summary>
        /// Hash a password using SHA256
        /// </summary>
        public string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("Password cannot be null or empty", nameof(password));
            }

            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        /// <summary>
        /// Verify if the provided password matches the hash
        /// </summary>
        public bool VerifyPassword(string password, string passwordHash)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(passwordHash))
            {
                return false;
            }

            var hashOfInput = HashPassword(password);
            return hashOfInput == passwordHash;
        }
    }
}

