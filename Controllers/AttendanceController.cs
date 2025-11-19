using Microsoft.AspNetCore.Mvc;
using attendance.DTOs;
using attendance.Services;
using attendance.Data;
using attendance.Models;
using Microsoft.EntityFrameworkCore;

namespace attendance.Controllers
{
    /// <summary>
    /// Controller for attendance operations (check-in, check-out)
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AttendanceController : ControllerBase
    {
        private readonly AttendanceDbContext _context;
        private readonly LocationService _locationService;
        private readonly AttendanceService _attendanceService;
        private readonly FileService _fileService;
        private readonly IConfiguration _configuration;

        public AttendanceController(
            AttendanceDbContext context,
            LocationService locationService,
            AttendanceService attendanceService,
            FileService fileService,
            IConfiguration configuration)
        {
            _context = context;
            _locationService = locationService;
            _attendanceService = attendanceService;
            _fileService = fileService;
            _configuration = configuration;
        }

        /// <summary>
        /// Check-in endpoint
        /// POST: api/attendance/checkin
        /// </summary>
        [HttpPost("checkin")]
        public async Task<ActionResult<ApiResponse<AttendanceResponse>>> CheckIn([FromForm] CheckInRequest request)
        {
            try
            {
                // Validate user exists
                var user = await _context.Users.FindAsync(request.UserId);
                if (user == null)
                {
                    return BadRequest(new ApiResponse<AttendanceResponse>
                    {
                        Success = false,
                        Message = "User not found"
                    });
                }

                // Check if already checked in today
                if (await _attendanceService.HasCheckedInTodayAsync(request.UserId))
                {
                    return BadRequest(new ApiResponse<AttendanceResponse>
                    {
                        Success = false,
                        Message = "You have already checked in today"
                    });
                }

                // Validate location (must be within 50m of office)
                var (isWithinRange, distance) = await _locationService.IsWithinOfficeRange(request.Latitude, request.Longitude);
                if (!isWithinRange)
                {
                    return BadRequest(new ApiResponse<AttendanceResponse>
                    {
                        Success = false,
                        Message = $"You are too far from the office. Distance: {Math.Round(distance, 2)} meters. Required: within 50 meters."
                    });
                }

                // Validate picture file - check for "picture" field name
                var pictureFile = Request.Form.Files["picture"] ?? Request.Form.Files.FirstOrDefault();
                if (pictureFile == null || pictureFile.Length == 0)
                {
                    return BadRequest(new ApiResponse<AttendanceResponse>
                    {
                        Success = false,
                        Message = "Check-in picture is required. Please send the picture file with field name 'picture'."
                    });
                }

                // Save picture
                var picturePath = await _fileService.SaveAttendancePictureAsync(pictureFile, request.UserId, "checkin");

                // Get working hours from configuration
                var workingHours = _configuration.GetSection("WorkingHours");
                var startTimeStr = workingHours.GetValue<string>("StartTime") ?? "09:00";
                var endTimeStr = workingHours.GetValue<string>("EndTime") ?? "18:00";
                
                var startTime = TimeSpan.Parse(startTimeStr);
                var endTime = TimeSpan.Parse(endTimeStr);
                var karachiTime = TimeZoneService.GetKarachiTime();
                var currentTime = karachiTime.TimeOfDay;
                
                // Check if check-in is late (arrives after the start time, e.g., after 9 AM)
                // If user arrives at 10 AM and start time is 9 AM, they are late
                var isLateCheckIn = currentTime > startTime;

                // Get or create today's attendance record
                var today = TimeZoneService.GetKarachiDate();
                var attendance = await _attendanceService.GetTodayAttendanceAsync(request.UserId);

                if (attendance == null)
                {
                    attendance = new Attendance
                    {
                        UserId = request.UserId,
                        Date = today,
                        CheckInTime = karachiTime,
                        CheckInLatitude = request.Latitude,
                        CheckInLongitude = request.Longitude,
                        CheckInPicturePath = picturePath,
                        IsPresent = true,
                        IsAbsent = false,
                        IsLateCheckIn = isLateCheckIn
                    };
                    _context.Attendances.Add(attendance);
                }
                else
                {
                    attendance.CheckInTime = TimeZoneService.GetKarachiTime();
                    attendance.CheckInLatitude = request.Latitude;
                    attendance.CheckInLongitude = request.Longitude;
                    attendance.CheckInPicturePath = picturePath;
                    attendance.IsPresent = true;
                    attendance.IsAbsent = false;
                    attendance.IsLateCheckIn = isLateCheckIn;
                }

                await _context.SaveChangesAsync();

                var response = new AttendanceResponse
                {
                    Id = attendance.Id,
                    UserId = attendance.UserId,
                    UserName = $"{user.FirstName} {user.LastName}",
                    Date = attendance.Date,
                    CheckInTime = attendance.CheckInTime,
                    CheckOutTime = attendance.CheckOutTime,
                    CheckInPicturePath = attendance.CheckInPicturePath,
                    IsPresent = attendance.IsPresent,
                    IsAbsent = attendance.IsAbsent,
                    IsLateCheckIn = attendance.IsLateCheckIn
                };

                var message = isLateCheckIn 
                    ? $"Check-in successful. Note: You checked in late (after {startTimeStr})." 
                    : "Check-in successful";

                return Ok(new ApiResponse<AttendanceResponse>
                {
                    Success = true,
                    Message = message,
                    Data = response
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<AttendanceResponse>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Check-out endpoint
        /// POST: api/attendance/checkout
        /// </summary>
        [HttpPost("checkout")]
        public async Task<ActionResult<ApiResponse<AttendanceResponse>>> CheckOut([FromForm] CheckOutRequest request)
        {
            try
            {
                // Validate user exists
                var user = await _context.Users.FindAsync(request.UserId);
                if (user == null)
                {
                    return BadRequest(new ApiResponse<AttendanceResponse>
                    {
                        Success = false,
                        Message = "User not found"
                    });
                }

                // Check if already checked out today
                if (await _attendanceService.HasCheckedOutTodayAsync(request.UserId))
                {
                    return BadRequest(new ApiResponse<AttendanceResponse>
                    {
                        Success = false,
                        Message = "You have already checked out today"
                    });
                }

                // Check if checked in today
                if (!await _attendanceService.HasCheckedInTodayAsync(request.UserId))
                {
                    return BadRequest(new ApiResponse<AttendanceResponse>
                    {
                        Success = false,
                        Message = "You must check in before checking out"
                    });
                }

                // Validate location (must be within 50m of office)
                var (isWithinRange, distance) = await _locationService.IsWithinOfficeRange(request.Latitude, request.Longitude);
                if (!isWithinRange)
                {
                    return BadRequest(new ApiResponse<AttendanceResponse>
                    {
                        Success = false,
                        Message = $"You are too far from the office. Distance: {Math.Round(distance, 2)} meters. Required: within 50 meters."
                    });
                }

                // Validate picture file - check for "picture" field name
                var pictureFile = Request.Form.Files["picture"] ?? Request.Form.Files.FirstOrDefault();
                if (pictureFile == null || pictureFile.Length == 0)
                {
                    return BadRequest(new ApiResponse<AttendanceResponse>
                    {
                        Success = false,
                        Message = "Check-out picture is required. Please send the picture file with field name 'picture'."
                    });
                }

                // Save picture
                var picturePath = await _fileService.SaveAttendancePictureAsync(pictureFile, request.UserId, "checkout");

                // Get working hours from configuration
                var workingHours = _configuration.GetSection("WorkingHours");
                var endTimeStr = workingHours.GetValue<string>("EndTime") ?? "18:00";
                var endTime = TimeSpan.Parse(endTimeStr);
                var karachiTime = TimeZoneService.GetKarachiTime();
                var currentTime = karachiTime.TimeOfDay;
                
                // Check if check-out is early (leaves before the end time, e.g., before 6 PM)
                // If user leaves at 5 PM and end time is 6 PM, they are early
                var isEarlyCheckOut = currentTime < endTime;

                // Get today's attendance record
                var attendance = await _attendanceService.GetTodayAttendanceAsync(request.UserId);
                if (attendance == null)
                {
                    return BadRequest(new ApiResponse<AttendanceResponse>
                    {
                        Success = false,
                        Message = "Attendance record not found"
                    });
                }

                attendance.CheckOutTime = karachiTime;
                attendance.CheckOutLatitude = request.Latitude;
                attendance.CheckOutLongitude = request.Longitude;
                attendance.CheckOutPicturePath = picturePath;
                attendance.IsEarlyCheckOut = isEarlyCheckOut;

                await _context.SaveChangesAsync();

                var response = new AttendanceResponse
                {
                    Id = attendance.Id,
                    UserId = attendance.UserId,
                    UserName = $"{user.FirstName} {user.LastName}",
                    Date = attendance.Date,
                    CheckInTime = attendance.CheckInTime,
                    CheckOutTime = attendance.CheckOutTime,
                    CheckInPicturePath = attendance.CheckInPicturePath,
                    CheckOutPicturePath = attendance.CheckOutPicturePath,
                    IsPresent = attendance.IsPresent,
                    IsAbsent = attendance.IsAbsent,
                    IsLateCheckIn = attendance.IsLateCheckIn,
                    IsEarlyCheckOut = attendance.IsEarlyCheckOut
                };

                var message = isEarlyCheckOut 
                    ? $"Check-out successful. Note: You checked out early (before {endTimeStr})." 
                    : "Check-out successful";

                return Ok(new ApiResponse<AttendanceResponse>
                {
                    Success = true,
                    Message = message,
                    Data = response
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<AttendanceResponse>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Get today's attendance status
        /// GET: api/attendance/today/{userId}
        /// </summary>
        [HttpGet("today/{userId}")]
        public async Task<ActionResult<ApiResponse<AttendanceResponse>>> GetTodayAttendance(int userId)
        {
            try
            {
                var attendance = await _attendanceService.GetTodayAttendanceAsync(userId);
                if (attendance == null)
                {
                    return Ok(new ApiResponse<AttendanceResponse>
                    {
                        Success = true,
                        Message = "No attendance record for today",
                        Data = null
                    });
                }

                var user = await _context.Users.FindAsync(userId);
                var response = new AttendanceResponse
                {
                    Id = attendance.Id,
                    UserId = attendance.UserId,
                    UserName = user != null ? $"{user.FirstName} {user.LastName}" : "",
                    Date = attendance.Date,
                    CheckInTime = attendance.CheckInTime,
                    CheckOutTime = attendance.CheckOutTime,
                    CheckInPicturePath = attendance.CheckInPicturePath,
                    CheckOutPicturePath = attendance.CheckOutPicturePath,
                    IsPresent = attendance.IsPresent,
                    IsAbsent = attendance.IsAbsent,
                    IsLateCheckIn = attendance.IsLateCheckIn,
                    IsEarlyCheckOut = attendance.IsEarlyCheckOut
                };

                return Ok(new ApiResponse<AttendanceResponse>
                {
                    Success = true,
                    Message = "Attendance retrieved successfully",
                    Data = response
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<AttendanceResponse>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Get monthly attendance with statistics
        /// GET: api/attendance/monthly/{userId}?year=2024&month=1
        /// </summary>
        [HttpGet("monthly/{userId}")]
        public async Task<ActionResult<ApiResponse<MonthlyAttendanceResponse>>> GetMonthlyAttendance(
            int userId,
            [FromQuery] int year = 0,
            [FromQuery] int month = 0)
        {
            try
            {
                // Default to current month if not specified
                if (year == 0) year = TimeZoneService.GetKarachiYear();
                if (month == 0) month = TimeZoneService.GetKarachiMonth();

                var monthlyAttendance = await _attendanceService.GetMonthlyAttendanceAsync(userId, year, month);

                return Ok(new ApiResponse<MonthlyAttendanceResponse>
                {
                    Success = true,
                    Message = "Monthly attendance retrieved successfully",
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
    }
}

