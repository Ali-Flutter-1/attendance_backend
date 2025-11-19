using attendance.Data;
using attendance.DTOs;
using attendance.Models;
using attendance.Services;
using Microsoft.EntityFrameworkCore;

namespace attendance.Services
{
    /// <summary>
    /// Service for attendance-related operations
    /// </summary>
    public class AttendanceService
    {
        private readonly AttendanceDbContext _context;

        public AttendanceService(AttendanceDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Check if user has already checked in today
        /// </summary>
        public async Task<bool> HasCheckedInTodayAsync(int userId)
        {
            var today = TimeZoneService.GetKarachiDate();
            return await _context.Attendances
                .AnyAsync(a => a.UserId == userId && a.Date == today && a.CheckInTime != null);
        }

        /// <summary>
        /// Check if user has already checked out today
        /// </summary>
        public async Task<bool> HasCheckedOutTodayAsync(int userId)
        {
            var today = TimeZoneService.GetKarachiDate();
            return await _context.Attendances
                .AnyAsync(a => a.UserId == userId && a.Date == today && a.CheckOutTime != null);
        }

        /// <summary>
        /// Get today's attendance record
        /// </summary>
        public async Task<Attendance?> GetTodayAttendanceAsync(int userId)
        {
            var today = TimeZoneService.GetKarachiDate();
            return await _context.Attendances
                .FirstOrDefaultAsync(a => a.UserId == userId && a.Date == today);
        }

        /// <summary>
        /// Get monthly attendance for a user
        /// </summary>
        public async Task<MonthlyAttendanceResponse> GetMonthlyAttendanceAsync(int userId, int year, int month)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new Exception("User not found");
            }

            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var attendances = await _context.Attendances
                .Where(a => a.UserId == userId && a.Date >= startDate && a.Date <= endDate)
                .OrderBy(a => a.Date)
                .ToListAsync();

            var totalPresent = attendances.Count(a => a.IsPresent);
            var totalAbsent = attendances.Count(a => a.IsAbsent);
            var totalDays = attendances.Count;

            var presentPercentage = totalDays > 0 ? (double)totalPresent / totalDays * 100 : 0;
            var absentPercentage = totalDays > 0 ? (double)totalAbsent / totalDays * 100 : 0;

            var dailyAttendances = attendances.Select(a => new AttendanceResponse
            {
                Id = a.Id,
                UserId = a.UserId,
                UserName = $"{user.FirstName} {user.LastName}",
                Date = a.Date,
                CheckInTime = a.CheckInTime,
                CheckOutTime = a.CheckOutTime,
                CheckInPicturePath = a.CheckInPicturePath,
                CheckOutPicturePath = a.CheckOutPicturePath,
                IsPresent = a.IsPresent,
                IsAbsent = a.IsAbsent
            }).ToList();

            return new MonthlyAttendanceResponse
            {
                UserId = userId,
                UserName = $"{user.FirstName} {user.LastName}",
                Year = year,
                Month = month,
                TotalPresent = totalPresent,
                TotalAbsent = totalAbsent,
                PresentPercentage = Math.Round(presentPercentage, 2),
                AbsentPercentage = Math.Round(absentPercentage, 2),
                DailyAttendances = dailyAttendances
            };
        }

        /// <summary>
        /// Mark absent users for a specific date
        /// This should be called daily (e.g., via a scheduled job)
        /// </summary>
        public async Task MarkAbsentUsersAsync(DateTime date)
        {
            var users = await _context.Users.ToListAsync();

            foreach (var user in users)
            {
                var attendance = await _context.Attendances
                    .FirstOrDefaultAsync(a => a.UserId == user.Id && a.Date == date.Date);

                // Check if user has an approved leave for this date
                var hasLeave = await _context.Leaves
                    .AnyAsync(l => l.UserId == user.Id &&
                                   l.Status == LeaveStatus.Approved &&
                                   date.Date >= l.StartDate && date.Date <= l.EndDate);

                if (attendance == null)
                {
                    // Create new attendance record as absent
                    attendance = new Attendance
                    {
                        UserId = user.Id,
                        Date = date.Date,
                        IsAbsent = !hasLeave, // If on leave, don't mark as absent
                        IsPresent = false
                    };
                    _context.Attendances.Add(attendance);
                }
                else if (!attendance.IsPresent && !hasLeave)
                {
                    // Update existing record if not present and not on leave
                    attendance.IsAbsent = true;
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}

