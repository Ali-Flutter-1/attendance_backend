using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using attendance.DTOs;
using attendance.Data;
using attendance.Services;
using attendance.Models;

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
                    var defaultStartDate = TimeZoneService.GetKarachiDate().AddDays(-30);
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
                if (year == 0) year = TimeZoneService.GetKarachiYear();
                if (month == 0) month = TimeZoneService.GetKarachiMonth();

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

    }

}

