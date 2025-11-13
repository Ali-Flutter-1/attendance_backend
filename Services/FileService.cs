namespace attendance.Services
{
    /// <summary>
    /// Service for handling file uploads (pictures)
    /// </summary>
    public class FileService
    {
        private readonly IWebHostEnvironment _environment;
        private const string UploadFolder = "uploads";
        private const string ProfilePicturesFolder = "profile-pictures";
        private const string AttendancePicturesFolder = "attendance-pictures";

        public FileService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        /// <summary>
        /// Save uploaded file and return the relative path
        /// </summary>
        public async Task<string> SaveFileAsync(IFormFile file, string subFolder)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File is empty");
            }

            // Validate file type (only images)
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                throw new ArgumentException("Invalid file type. Only images are allowed.");
            }

            // Create directory if it doesn't exist
            var uploadPath = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, UploadFolder, subFolder);
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            // Generate unique filename
            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadPath, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return relative path
            return $"{UploadFolder}/{subFolder}/{fileName}";
        }

        /// <summary>
        /// Save profile picture
        /// </summary>
        public async Task<string> SaveProfilePictureAsync(IFormFile file, int userId)
        {
            return await SaveFileAsync(file, $"{ProfilePicturesFolder}/{userId}");
        }

        /// <summary>
        /// Save attendance picture (check-in or check-out)
        /// </summary>
        public async Task<string> SaveAttendancePictureAsync(IFormFile file, int userId, string type)
        {
            return await SaveFileAsync(file, $"{AttendancePicturesFolder}/{userId}/{type}");
        }

        /// <summary>
        /// Delete file
        /// </summary>
        public void DeleteFile(string? filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return;

            var fullPath = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, filePath);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
    }
}

