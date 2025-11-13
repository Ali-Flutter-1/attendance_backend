using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using attendance.DTOs;
using attendance.Data;
using attendance.Services;

namespace attendance.Controllers
{
    /// <summary>
    /// Controller for admin operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly AttendanceDbContext _context;
        private readonly AttendanceService _attendanceService;

        public AdminController(AttendanceDbContext context, AttendanceService attendanceService)
        {
            _context = context;
            _attendanceService = attendanceService;
        }

        /// <summary>
        /// Create a new user (for testing/setup)
        /// POST: api/admin/users
        /// </summary>
        [HttpPost("users")]
        public async Task<ActionResult<ApiResponse<UserResponse>>> CreateUser([FromBody] CreateUserRequest request)
        {
            try
            {
                // Check if email already exists
                var emailExists = await _context.Users.AnyAsync(u => u.Email == request.Email);
                if (emailExists)
                {
                    return BadRequest(new ApiResponse<UserResponse>
                    {
                        Success = false,
                        Message = "Email already exists"
                    });
                }

                var user = new Models.User
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email,
                    Domain = request.Domain,
                    Address = request.Address,
                    IsAdmin = request.IsAdmin ?? false
                };

                _context.Users.Add(user);
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
                    Message = "User created successfully",
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
        /// Get all users (admin only)
        /// GET: api/admin/users
        /// </summary>
        [HttpGet("users")]
        public async Task<ActionResult<ApiResponse<List<UserResponse>>>> GetAllUsers()
        {
            try
            {
                var users = await _context.Users
                    .OrderBy(u => u.FirstName)
                    .ThenBy(u => u.LastName)
                    .ToListAsync();

                var response = users.Select(u => new UserResponse
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    Domain = u.Domain,
                    Address = u.Address,
                    ProfilePicturePath = u.ProfilePicturePath,
                    IsAdmin = u.IsAdmin
                }).ToList();

                return Ok(new ApiResponse<List<UserResponse>>
                {
                    Success = true,
                    Message = "Users retrieved successfully",
                    Data = response
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<UserResponse>>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Get all user activities (attendance records)
        /// GET: api/admin/activities?startDate=2024-01-01&endDate=2024-01-31&userId=1
        /// </summary>
        [HttpGet("activities")]
        public async Task<ActionResult<ApiResponse<List<AttendanceResponse>>>> GetUserActivities(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int? userId = null)
        {
            try
            {
                var query = _context.Attendances.Include(a => a.User).AsQueryable();

                // Filter by user if provided
                if (userId.HasValue)
                {
                    query = query.Where(a => a.UserId == userId.Value);
                }

                // Filter by date range if provided
                if (startDate.HasValue)
                {
                    query = query.Where(a => a.Date >= startDate.Value.Date);
                }

                if (endDate.HasValue)
                {
                    query = query.Where(a => a.Date <= endDate.Value.Date);
                }

                // Default to last 30 days if no dates provided
                if (!startDate.HasValue && !endDate.HasValue)
                {
                    var defaultStartDate = DateTime.UtcNow.AddDays(-30).Date;
                    query = query.Where(a => a.Date >= defaultStartDate);
                }

                var attendances = await query
                    .OrderByDescending(a => a.Date)
                    .ThenBy(a => a.User.FirstName)
                    .ToListAsync();

                var response = attendances.Select(a => new AttendanceResponse
                {
                    Id = a.Id,
                    UserId = a.UserId,
                    UserName = $"{a.User.FirstName} {a.User.LastName}",
                    Date = a.Date,
                    CheckInTime = a.CheckInTime,
                    CheckOutTime = a.CheckOutTime,
                    CheckInPicturePath = a.CheckInPicturePath,
                    CheckOutPicturePath = a.CheckOutPicturePath,
                    IsPresent = a.IsPresent,
                    IsAbsent = a.IsAbsent
                }).ToList();

                return Ok(new ApiResponse<List<AttendanceResponse>>
                {
                    Success = true,
                    Message = "Activities retrieved successfully",
                    Data = response
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<AttendanceResponse>>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Get user statistics
        /// GET: api/admin/statistics/{userId}?year=2024&month=1
        /// </summary>
        [HttpGet("statistics/{userId}")]
        public async Task<ActionResult<ApiResponse<MonthlyAttendanceResponse>>> GetUserStatistics(
            int userId,
            [FromQuery] int year = 0,
            [FromQuery] int month = 0)
        {
            try
            {
                if (year == 0) year = DateTime.UtcNow.Year;
                if (month == 0) month = DateTime.UtcNow.Month;

                var monthlyAttendance = await _attendanceService.GetMonthlyAttendanceAsync(userId, year, month);

                return Ok(new ApiResponse<MonthlyAttendanceResponse>
                {
                    Success = true,
                    Message = "Statistics retrieved successfully",
                    Data = monthlyAttendance
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<MonthlyAttendanceResponse>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Get current office location
        /// GET: api/admin/office-location
        /// </summary>
        [HttpGet("office-location")]
        public async Task<ActionResult<ApiResponse<object>>> GetOfficeLocation()
        {
            try
            {
                var location = await _context.OfficeLocations
                    .Where(o => o.IsActive)
                    .FirstOrDefaultAsync();

                if (location == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Office location not configured"
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Office location retrieved successfully",
                    Data = new
                    {
                        location.Id,
                        location.Name,
                        location.Latitude,
                        location.Longitude,
                        location.AllowedRadiusInMeters,
                        location.IsActive
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Configure office location
        /// POST: api/admin/office-location
        /// </summary>
        [HttpPost("office-location")]
        public async Task<ActionResult<ApiResponse<object>>> SetOfficeLocation(
            [FromBody] OfficeLocationRequest request)
        {
            try
            {
                // Deactivate all existing locations
                var existingLocations = await _context.OfficeLocations.Where(o => o.IsActive).ToListAsync();
                foreach (var loc in existingLocations)
                {
                    loc.IsActive = false;
                }

                // Create or update active location
                var location = await _context.OfficeLocations
                    .FirstOrDefaultAsync(l => l.Latitude == request.Latitude && l.Longitude == request.Longitude);

                if (location == null)
                {
                    location = new Models.OfficeLocation
                    {
                        Name = request.Name ?? "Main Office",
                        Latitude = request.Latitude,
                        Longitude = request.Longitude,
                        AllowedRadiusInMeters = request.AllowedRadiusInMeters,
                        IsActive = true
                    };
                    _context.OfficeLocations.Add(location);
                }
                else
                {
                    location.Name = request.Name ?? location.Name;
                    location.AllowedRadiusInMeters = request.AllowedRadiusInMeters;
                    location.IsActive = true;
                }

                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Office location configured successfully",
                    Data = new
                    {
                        location.Id,
                        location.Name,
                        location.Latitude,
                        location.Longitude,
                        location.AllowedRadiusInMeters,
                        location.IsActive
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                });
            }
        }
    }

    /// <summary>
    /// DTO for office location configuration
    /// </summary>
    public class OfficeLocationRequest
    {
        public string? Name { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double AllowedRadiusInMeters { get; set; } = 50.0;
    }

    /// <summary>
    /// DTO for creating a new user
    /// </summary>
    public class CreateUserRequest
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Domain { get; set; }
        public string? Address { get; set; }
        public bool? IsAdmin { get; set; }
    }
}

