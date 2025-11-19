# Office Location Setup Guide

## Default Office Location Configuration

The office location is now configured in `appsettings.json` and will be automatically initialized when the API starts.

## Step 1: Update Office Coordinates

Open `appsettings.json` and update the coordinates to your actual office location:

```json
{
  "OfficeLocation": {
    "Name": "Main Office",
    "Latitude": 31.413239,      // Replace with your office latitude
    "Longitude": -73.989308,      // Replace with your office longitude
    "AllowedRadiusInMeters": 50  // Distance in meters (default: 50m)
  }
}
```

### How to Get Your Office Coordinates

1. **Using Google Maps:**
   - Open Google Maps
   - Search for your office address
   - Right-click on the location
   - Click on the coordinates (e.g., "24.8607, 67.0011")
   - Copy the latitude and longitude

2. **Using GPS on Your Phone:**
   - Open Google Maps app
   - Navigate to your office
   - Long press on the location
   - The coordinates will appear at the bottom

3. **Using Online Tools:**
   - Visit: https://www.latlong.net/
   - Enter your address
   - Get the coordinates

## Step 2: Restart the API

After updating `appsettings.json`:

1. Stop the running API (Ctrl+C)
2. Restart it: `dotnet run`
3. Check the console - you should see: "Office location initialized successfully."

## Step 3: Verify Office Location

### Option 1: Using Swagger UI

1. Go to `https://localhost:7225/swagger`
2. Find `GET /api/admin/office-location`
3. Click "Try it out" → "Execute"
4. Verify the coordinates match your office

### Option 2: Using Browser/Postman

```http
GET https://localhost:7225/api/admin/office-location
```

**Response:**
```json
{
  "success": true,
  "message": "Office location retrieved successfully",
  "data": {
    "id": 1,
    "name": "Main Office",
    "latitude": 31.413239,
    "longitude": 67.0011,
    "allowedRadiusInMeters": 50,
    "isActive": true
  }
}
```

## Step 4: Update Office Location via SQL (Optional)

You can also update the office location directly in the database using SQL. See `UPDATE_OFFICE_LOCATION.sql` for the SQL script.

## How It Works

1. **On API Startup:**
   - The `OfficeLocationService` checks if an office location exists
   - If not found, it reads from `appsettings.json`
   - Creates a new office location record in the database
   - Sets it as active

2. **When User Checks In:**
   - Flutter app gets user's GPS coordinates
   - Sends coordinates to API
   - API calculates distance using Haversine formula
   - If within 50m (or configured radius), check-in succeeds
   - Otherwise, returns error with distance

## Example Coordinates

Here are some example coordinates for major cities:

- **Karachi, Pakistan:** 24.8607, 67.0011
- **Lahore, Pakistan:** 31.5204, 74.3587
- **Islamabad, Pakistan:** 33.6844, 73.0479
- **New York, USA:** 40.7128, -74.0060
- **London, UK:** 51.5074, -0.1278
- **Tokyo, Japan:** 35.6762, 139.6503

## Troubleshooting

### Office Location Not Initialized

**Problem:** Console shows "Could not initialize office location"

**Solutions:**
1. Check that `appsettings.json` has valid coordinates
2. Ensure database connection is working
3. Check that database exists and migrations are applied
4. Verify SQL Server is running

### Wrong Coordinates

**Problem:** Office location has wrong coordinates

**Solutions:**
1. Update `appsettings.json` with correct coordinates
2. Restart the API
3. Or update directly in the database using SQL (see `UPDATE_OFFICE_LOCATION.sql`)

### Location Verification Not Working

**Problem:** Users can't check in even when at office

**Solutions:**
1. Verify office coordinates are correct
2. Check the allowed radius (default 50m)
3. Ensure user's GPS is accurate (use high accuracy mode)
4. Check API logs for distance calculation

## Testing Location Verification

1. **Get Office Location:**
   ```http
   GET /api/admin/office-location
   ```

2. **Test Check-In with Office Coordinates:**
   ```http
   POST /api/attendance/checkin
   Form Data:
   - userId: 1
   - latitude: [same as office latitude]
   - longitude: [same as office longitude]
   - picture: [image file]
   ```

3. **Test Check-In from Far Away:**
   ```http
   POST /api/attendance/checkin
   Form Data:
   - userId: 1
   - latitude: 25.0000  (different coordinates)
   - longitude: 68.0000
   - picture: [image file]
   ```
   Should return error: "You are too far from the office"

## Notes

- **Default Radius:** 50 meters (can be changed in `appsettings.json`)
- **Accuracy:** Uses high-accuracy GPS (recommended for Flutter app)
- **Distance Calculation:** Uses Haversine formula (accounts for Earth's curvature)
- **Multiple Offices:** Currently supports one active office location. For multiple offices, you'll need to extend the system.

