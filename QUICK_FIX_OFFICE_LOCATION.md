# Quick Fix: Update Office Location

## Problem
Your device location (31.4132529, 73.0988297) is very close to the office (31.413239, 73.0988347), but the API is calculating a distance of 11,307 km. This means the database still has the old office coordinates (probably New York: 40.741895, -73.989308).

## Solution: Update Office Location

You have 3 options:

### Option 1: Use API Endpoint (Easiest)

```http
POST https://localhost:7225/api/admin/office-location
Content-Type: application/json

{
  "name": "Main Office",
  "latitude": 31.413239,
  "longitude": 73.0988347,
  "allowedRadiusInMeters": 50
}
```

### Option 2: Run SQL Script (Fastest)

1. Open SQL Server Management Studio
2. Connect to your database
3. Run the file: `UPDATE_OFFICE_LOCATION.sql`
4. Done! ✅

### Option 3: Manual SQL Update

```sql
USE ATTENDANCE;
GO

UPDATE OfficeLocations
SET 
    Latitude = 31.413239,
    Longitude = 73.0988347,
    AllowedRadiusInMeters = 50.0
WHERE IsActive = 1;
GO
```

## Verify It Worked

After updating, test the check-in again. The distance should now be:
- **Expected distance:** ~1-2 meters (very close!)
- **Your location:** 31.4132529, 73.0988297
- **Office location:** 31.413239, 73.0988347
- **Difference:** Only 0.0000139° in latitude and 0.000005° in longitude

## Test Distance Calculation

Your coordinates are:
- **Latitude difference:** 31.4132529 - 31.413239 = 0.0000139°
- **Longitude difference:** 73.0988297 - 73.0988347 = -0.000005°

This is approximately **1.5 meters** distance, which is well within the 50-meter radius!

After updating the office location in the database, your check-in should work perfectly! ✅

