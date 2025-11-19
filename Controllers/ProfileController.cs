using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using attendance.DTOs;
using attendance.Data;
using attendance.Services;

namespace attendance.Controllers
{
    /// <summary>
    /// Controller for user profile management
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ProfileController : ControllerBase
    {
        private readonly AttendanceDbContext _context;
        private readonly FileService _fileService;

        public ProfileController(AttendanceDbContext context, FileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        /// <summary>
        /// Get user profile
        /// GET: api/profile/{userId}
        /// </summary>
        [HttpGet("{userId}")]
        public async Task<ActionResult<ApiResponse<UserResponse>>> GetProfile(int userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return NotFound(new ApiResponse<UserResponse>
                    {
                        Success = false,
                        Message = "User not found"
                    });
                }

                var response = new UserResponse
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Domain = user.Domain,
                    Address = user.Address,
                    ProfilePicturePath = user.ProfilePicturePath,
                    IsAdmin = user.IsAdmin
                };

                return Ok(new ApiResponse<UserResponse>
                {
                    Success = true,
                    Message = "Profile retrieved successfully",
                    Data = response
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<UserResponse>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Update user profile
        /// PUT: api/profile/update
        /// </summary>
        [HttpPut("update")]
        public async Task<ActionResult<ApiResponse<UserResponse>>> UpdateProfile([FromForm] UpdateProfileRequest request)
        {
            try
            {
                var user = await _context.Users.FindAsync(request.UserId);
                if (user == null)
                {
                    return NotFound(new ApiResponse<UserResponse>
                    {
                        Success = false,
                        Message = "User not found"
                    });
                }

                // Update fields if provided
                if (!string.IsNullOrEmpty(request.FirstName))
                    user.FirstName = request.FirstName;

                if (!string.IsNullOrEmpty(request.LastName))
                    user.LastName = request.LastName;

                if (!string.IsNullOrEmpty(request.Email))
                {
                    // Check if email is already taken by another user
                    var emailExists = await _context.Users
                        .AnyAsync(u => u.Email == request.Email && u.Id != request.UserId);
                    
                    if (emailExists)
                    {
                        return BadRequest(new ApiResponse<UserResponse>
                        {
                            Success = false,
                            Message = "Email is already taken"
                        });
                    }
                    user.Email = request.Email;
                }

                if (request.Domain != null)
                    user.Domain = request.Domain;

                // Handle profile picture upload - check for "picture" field name
                var pictureFile = Request.Form.Files["picture"] ?? Request.Form.Files.FirstOrDefault();
                if (pictureFile != null && pictureFile.Length > 0)
                {
                    // Delete old picture if exists
                    if (!string.IsNullOrEmpty(user.ProfilePicturePath))
                    {
                        _fileService.DeleteFile(user.ProfilePicturePath);
                    }

                    // Save new picture
                    user.ProfilePicturePath = await _fileService.SaveProfilePictureAsync(pictureFile, request.UserId);
                }

                user.UpdatedAt = TimeZoneService.GetKarachiTime();
                await _context.SaveChangesAsync();

                var response = new UserResponse
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Domain = user.Domain,
                    Address = user.Address,
                    ProfilePicturePath = user.ProfilePicturePath,
                    IsAdmin = user.IsAdmin
                };

                return Ok(new ApiResponse<UserResponse>
                {
                    Success = true,
                    Message = "Profile updated successfully",
                    Data = response
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<UserResponse>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                });
            }
        }
    }
}

