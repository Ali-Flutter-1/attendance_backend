using attendance.Data;
using attendance.Models;
using Microsoft.EntityFrameworkCore;

namespace attendance.Services
{
    /// <summary>
    /// Service for managing office location initialization
    /// </summary>
    public class OfficeLocationService
    {
        private readonly AttendanceDbContext _context;
        private readonly IConfiguration _configuration;

        public OfficeLocationService(AttendanceDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        /// <summary>
        /// Initialize or update office location from appsettings.json
        /// </summary>
        public async Task InitializeOfficeLocationAsync()
        {
            // Get default location from appsettings.json
            var officeConfig = _configuration.GetSection("OfficeLocation");
            var latitude = officeConfig.GetValue<double>("Latitude");
            var longitude = officeConfig.GetValue<double>("Longitude");
            var name = officeConfig.GetValue<string>("Name") ?? "Main Office";
            var radius = officeConfig.GetValue<double>("AllowedRadiusInMeters");

            // Check if office location already exists
            var existingLocation = await _context.OfficeLocations
                .Where(o => o.IsActive)
                .FirstOrDefaultAsync();

            if (existingLocation == null)
            {
                // Create new office location
                var officeLocation = new OfficeLocation
                {
                    Name = name,
                    Latitude = latitude,
                    Longitude = longitude,
                    AllowedRadiusInMeters = radius > 0 ? radius : 50.0,
                    IsActive = true
                };

                _context.OfficeLocations.Add(officeLocation);
                await _context.SaveChangesAsync();
            }
            else
            {
                // Update existing location if coordinates or radius changed
                var coordinatesChanged = Math.Abs(existingLocation.Latitude - latitude) > 0.000001 ||
                                       Math.Abs(existingLocation.Longitude - longitude) > 0.000001;
                var radiusChanged = Math.Abs(existingLocation.AllowedRadiusInMeters - radius) > 0.01;

                if (coordinatesChanged || radiusChanged)
                {
                    existingLocation.Latitude = latitude;
                    existingLocation.Longitude = longitude;
                    existingLocation.AllowedRadiusInMeters = radius > 0 ? radius : 50.0;
                    existingLocation.Name = name;
                    await _context.SaveChangesAsync();
                }
            }
        }
    }
}

