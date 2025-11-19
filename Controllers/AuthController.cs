using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using attendance.DTOs;
using attendance.Data;
using attendance.Services;

namespace attendance.Controllers
{
    /// <summary>
    /// Controller for authentication (signup and login)
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AttendanceDbContext _context;
        private readonly PasswordService _passwordService;
        private readonly FileService _fileService;

        public AuthController(AttendanceDbContext context, PasswordService passwordService, FileService fileService)
        {
            _context = context;
            _passwordService = passwordService;
            _fileService = fileService;
        }

        /// <summary>
        /// User signup with full profile information
        /// POST: api/auth/signup
        /// Content-Type: multipart/form-data
        /// </summary>
        [HttpPost("signup")]
        public async Task<ActionResult<ApiResponse<LoginResponse>>> Signup([FromForm] SignupRequest request)
        {
            try
            {
                // Validate model
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<LoginResponse>
                    {
                        Success = false,
                        Message = "Invalid input data"
                    });
                }

                // Check if passwords match
                if (request.Password != request.ConfirmPassword)
                {
                    return BadRequest(new ApiResponse<LoginResponse>
                    {
                        Success = false,
                        Message = "Password and confirm password do not match"
                    });
                }

                // Check if email already exists
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
                if (existingUser != null)
                {
                    // If user exists but has no password, allow setting password and updating profile
                    if (string.IsNullOrEmpty(existingUser.PasswordHash))
                    {
                        // Update existing user with signup data
                        existingUser.PasswordHash = _passwordService.HashPassword(request.Password);
                        existingUser.FirstName = request.FirstName;
                        existingUser.LastName = request.LastName;
                        existingUser.Domain = request.Domain;
                        existingUser.Address = request.Address;

                        // Handle profile picture upload
                        var picturefile = Request.Form.Files["picture"] ?? Request.Form.Files.FirstOrDefault();
                        if (picturefile != null && picturefile.Length > 0)
                        {
                            // Delete old picture if exists
                            if (!string.IsNullOrEmpty(existingUser.ProfilePicturePath))
                            {
                                _fileService.DeleteFile(existingUser.ProfilePicturePath);
                            }
                            existingUser.ProfilePicturePath = await _fileService.SaveProfilePictureAsync(picturefile, existingUser.Id);
                        }

                        existingUser.UpdatedAt = TimeZoneService.GetKarachiTime();
                        await _context.SaveChangesAsync();

                        var existingUserResponse = new LoginResponse
                        {
                            Id = existingUser.Id,
                            FirstName = existingUser.FirstName,
                            LastName = existingUser.LastName,
                            Email = existingUser.Email,
                            Domain = existingUser.Domain,
                            Address = existingUser.Address,
                            ProfilePicturePath = existingUser.ProfilePicturePath,
                            IsAdmin = existingUser.IsAdmin
                        };

                        return Ok(new ApiResponse<LoginResponse>
                        {
                            Success = true,
                            Message = "Account setup completed successfully. You can now login.",
                            Data = existingUserResponse
                        });
                    }
                    else
                    {
                        return BadRequest(new ApiResponse<LoginResponse>
                        {
                            Success = false,
                            Message = "Email already exists. Please use a different email or login."
                        });
                    }
                }

                // Validate password strength
                if (request.Password.Length < 6)
                {
                    return BadRequest(new ApiResponse<LoginResponse>
                    {
                        Success = false,
                        Message = "Password must be at least 6 characters long"
                    });
                }

                // Hash password
                var passwordHash = _passwordService.HashPassword(request.Password);

                // Handle profile picture upload
                string? profilePicturePath = null;
                var pictureFile = Request.Form.Files["picture"] ?? Request.Form.Files.FirstOrDefault();
                if (pictureFile != null && pictureFile.Length > 0)
                {
                    // We'll save it after creating the user (need userId first)
                    // For now, we'll create user first, then update with picture path
                }

                // Create new user
                var user = new Models.User
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email,
                    PasswordHash = passwordHash,
                    Domain = request.Domain,
                    Address = request.Address,
                    IsAdmin = false,
                    CreatedAt = TimeZoneService.GetKarachiTime()
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Save profile picture after user is created (so we have userId)
                if (pictureFile != null && pictureFile.Length > 0)
                {
                    user.ProfilePicturePath = await _fileService.SaveProfilePictureAsync(pictureFile, user.Id);
                    await _context.SaveChangesAsync();
                }

                // Return user data (without password)
                var response = new LoginResponse
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

                return Ok(new ApiResponse<LoginResponse>
                {
                    Success = true,
                    Message = "Account created successfully",
                    Data = response
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<LoginResponse>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// User login
        /// POST: api/auth/login
        /// </summary>
        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<LoginResponse>>> Login([FromBody] LoginRequest request)
        {
            try
            {
                // Validate model
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<LoginResponse>
                    {
                        Success = false,
                        Message = "Invalid input data"
                    });
                }

                // Find user by email
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
                if (user == null)
                {
                    return Unauthorized(new ApiResponse<LoginResponse>
                    {
                        Success = false,
                        Message = "Invalid email or password"
                    });
                }

                // Check if user has a password set
                if (string.IsNullOrEmpty(user.PasswordHash))
                {
                    return BadRequest(new ApiResponse<LoginResponse>
                    {
                        Success = false,
                        Message = "Password not set. Please signup first to set your password."
                    });
                }

                // Verify password
                if (!_passwordService.VerifyPassword(request.Password, user.PasswordHash))
                {
                    return Unauthorized(new ApiResponse<LoginResponse>
                    {
                        Success = false,
                        Message = "Invalid email or password"
                    });
                }

                // Return user data (without password)
                var response = new LoginResponse
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

                return Ok(new ApiResponse<LoginResponse>
                {
                    Success = true,
                    Message = "Login successful",
                    Data = response
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<LoginResponse>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                });
            }
        }
    }
}

