using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using attendance.DTOs;
using attendance.Data;
using attendance.Models;
using attendance.Services;

namespace attendance.Controllers
{
    /// <summary>
    /// Controller for leave management
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class LeaveController : ControllerBase
    {
        private readonly AttendanceDbContext _context;

        public LeaveController(AttendanceDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Apply for leave
        /// POST: api/leave/apply
        /// </summary>
        [HttpPost("apply")]
        public async Task<ActionResult<ApiResponse<LeaveResponse>>> ApplyForLeave([FromBody] LeaveRequest request)
        {
            try
            {
                // Validate user exists
                var user = await _context.Users.FindAsync(request.UserId);
                if (user == null)
                {
                    return BadRequest(new ApiResponse<LeaveResponse>
                    {
                        Success = false,
                        Message = "User not found"
                    });
                }

                // Validate dates
                if (request.StartDate > request.EndDate)
                {
                    return BadRequest(new ApiResponse<LeaveResponse>
                    {
                        Success = false,
                        Message = "Start date cannot be after end date"
                    });
                }

                if (request.StartDate < TimeZoneService.GetKarachiDate())
                {
                    return BadRequest(new ApiResponse<LeaveResponse>
                    {
                        Success = false,
                        Message = "Start date cannot be in the past"
                    });
                }

                // Check if leave type is valid
                if (!Enum.IsDefined(typeof(LeaveType), request.Type))
                {
                    return BadRequest(new ApiResponse<LeaveResponse>
                    {
                        Success = false,
                        Message = "Invalid leave type"
                    });
                }

                // Create leave application
                var leave = new Leave
                {
                    UserId = request.UserId,
                    Type = (LeaveType)request.Type,
                    Reason = request.Reason,
                    StartDate = request.StartDate.Date,
                    EndDate = request.EndDate.Date,
                    Status = LeaveStatus.Pending
                };

                _context.Leaves.Add(leave);
                await _context.SaveChangesAsync();

                var response = new LeaveResponse
                {
                    Id = leave.Id,
                    UserId = leave.UserId,
                    UserName = $"{user.FirstName} {user.LastName}",
                    Type = leave.Type.ToString(),
                    Reason = leave.Reason,
                    StartDate = leave.StartDate,
                    EndDate = leave.EndDate,
                    Status = leave.Status.ToString(),
                    CreatedAt = leave.CreatedAt
                };

                return Ok(new ApiResponse<LeaveResponse>
                {
                    Success = true,
                    Message = "Leave application submitted successfully",
                    Data = response
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<LeaveResponse>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Get user's leave applications
        /// GET: api/leave/user/{userId}
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<ApiResponse<List<LeaveResponse>>>> GetUserLeaves(int userId)
        {
            try
            {
                var leaves = await _context.Leaves
                    .Include(l => l.User)
                    .Where(l => l.UserId == userId)
                    .OrderByDescending(l => l.CreatedAt)
                    .ToListAsync();

                var response = leaves.Select(l => new LeaveResponse
                {
                    Id = l.Id,
                    UserId = l.UserId,
                    UserName = $"{l.User.FirstName} {l.User.LastName}",
                    Type = l.Type.ToString(),
                    Reason = l.Reason,
                    StartDate = l.StartDate,
                    EndDate = l.EndDate,
                    Status = l.Status.ToString(),
                    AdminRemarks = l.AdminRemarks,
                    CreatedAt = l.CreatedAt
                }).ToList();

                return Ok(new ApiResponse<List<LeaveResponse>>
                {
                    Success = true,
                    Message = "Leaves retrieved successfully",
                    Data = response
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<LeaveResponse>>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                });
            }
        }




        /// <summary>
        /// Get all pending leaves (for admin)
        /// GET: api/leave/pending
        /// </summary>
        [HttpGet("pending")]
        public async Task<ActionResult<ApiResponse<List<LeaveResponse>>>> GetPendingLeaves()
        {
            try
            {
                var leaves = await _context.Leaves
                    .Include(l => l.User)
                    .Where(l => l.Status == LeaveStatus.Pending)
                    .OrderByDescending(l => l.CreatedAt)
                    .ToListAsync();

                var response = leaves.Select(l => new LeaveResponse
                {
                    Id = l.Id,
                    UserId = l.UserId,
                    UserName = $"{l.User.FirstName} {l.User.LastName}",
                    Type = l.Type.ToString(),
                    Reason = l.Reason,
                    StartDate = l.StartDate,
                    EndDate = l.EndDate,
                    Status = l.Status.ToString(),
                    AdminRemarks = l.AdminRemarks,
                    CreatedAt = l.CreatedAt
                }).ToList();

                return Ok(new ApiResponse<List<LeaveResponse>>
                {
                    Success = true,
                    Message = "Pending leaves retrieved successfully",
                    Data = response
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<LeaveResponse>>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Approve or decline leave (admin only)
        /// POST: api/leave/approve
        /// </summary>
        [HttpPost("approve")]
        public async Task<ActionResult<ApiResponse<LeaveResponse>>> ApproveLeave([FromBody] LeaveApprovalRequest request)
        {
            try
            {
                var leave = await _context.Leaves
                    .Include(l => l.User)
                    .FirstOrDefaultAsync(l => l.Id == request.LeaveId);

                if (leave == null)
                {
                    return BadRequest(new ApiResponse<LeaveResponse>
                    {
                        Success = false,
                        Message = "Leave application not found"
                    });
                }

                if (leave.Status != LeaveStatus.Pending)
                {
                    return BadRequest(new ApiResponse<LeaveResponse>
                    {
                        Success = false,
                        Message = "Leave application has already been processed"
                    });
                }

                // Validate status
                if (request.Status != (int)LeaveStatus.Approved && request.Status != (int)LeaveStatus.Declined)
                {
                    return BadRequest(new ApiResponse<LeaveResponse>
                    {
                        Success = false,
                        Message = "Invalid status. Use 2 for Approved or 3 for Declined"
                    });
                }

                leave.Status = (LeaveStatus)request.Status;
                leave.AdminRemarks = request.AdminRemarks;
                leave.UpdatedAt = TimeZoneService.GetKarachiTime();

                await _context.SaveChangesAsync();

                var response = new LeaveResponse
                {
                    Id = leave.Id,
                    UserId = leave.UserId,
                    UserName = $"{leave.User.FirstName} {leave.User.LastName}",
                    Type = leave.Type.ToString(),
                    Reason = leave.Reason,
                    StartDate = leave.StartDate,
                    EndDate = leave.EndDate,
                    Status = leave.Status.ToString(),
                    AdminRemarks = leave.AdminRemarks,
                    CreatedAt = leave.CreatedAt
                };

                return Ok(new ApiResponse<LeaveResponse>
                {
                    Success = true,
                    Message = $"Leave application {(leave.Status == LeaveStatus.Approved ? "approved" : "declined")} successfully",
                    Data = response
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<LeaveResponse>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                });
            }
        }
    }
}

