using attendance.Data;
using attendance.Models;
using Microsoft.EntityFrameworkCore;

namespace attendance.Services
{
    /// <summary>
    /// Service for location-based operations
    /// Calculates distance between two GPS coordinates using Haversine formula
    /// </summary>
    public class LocationService
    {
        private readonly AttendanceDbContext _context;

        public LocationService(AttendanceDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Calculate distance between two GPS coordinates in meters using Haversine formula
        /// </summary>
        public double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371000; // Earth's radius in meters
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var distance = R * c;

            return distance;
        }

        private double ToRadians(double degrees)
        {
            return degrees * (Math.PI / 180);
        }

        /// <summary>
        /// Check if user location is within allowed radius of office
        /// </summary>
        public async Task<(bool IsWithinRange, double DistanceInMeters)> IsWithinOfficeRange(double userLat, double userLon)
        {
            var officeLocation = await _context.OfficeLocations
                .Where(o => o.IsActive)
                .FirstOrDefaultAsync();

            if (officeLocation == null)
            {
                throw new Exception("Office location not configured");
            }

            var distance = CalculateDistance(userLat, userLon, officeLocation.Latitude, officeLocation.Longitude);
            var isWithinRange = distance <= officeLocation.AllowedRadiusInMeters;

            return (isWithinRange, distance);
        }

        /// <summary>
        /// Get active office location
        /// </summary>
        public async Task<OfficeLocation?> GetActiveOfficeLocationAsync()
        {
            return await _context.OfficeLocations
                .Where(o => o.IsActive)
                .FirstOrDefaultAsync();
        }
    }
}

