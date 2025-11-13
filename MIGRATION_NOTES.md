# Database Migration Required

## New Fields Added

The following fields have been added to the `Attendance` table:

1. **IsLateCheckIn** (bool) - Tracks if user checked in after 9 AM
2. **IsEarlyCheckOut** (bool) - Tracks if user checked out before 6 PM

## Migration Steps

Run these commands to update your database:

```bash
# Create a new migration
dotnet ef migrations add AddLateEarlyTracking

# Apply the migration
dotnet ef database update
```

## What Changed

### 1. Picture Field Name
- The API now expects the picture file to be sent with field name **"picture"**
- Example in Flutter: `request.files.add(await http.MultipartFile.fromPath('picture', imagePath));`

### 2. Working Hours Configuration
- Added to `appsettings.json`:
  ```json
  "WorkingHours": {
    "StartTime": "09:00",
    "EndTime": "18:00"
  }
  ```

### 3. Late/Early Tracking
- **IsLateCheckIn**: `true` if check-in time is after 9:00 AM
- **IsEarlyCheckOut**: `true` if check-out time is before 6:00 PM (18:00)

### 4. Response Messages
- Check-in: Shows warning if late
- Check-out: Shows warning if early

## API Response Changes

The `AttendanceResponse` now includes:
```json
{
  "isLateCheckIn": true/false,
  "isEarlyCheckOut": true/false
}
```

## Flutter Update Required

Update your Flutter code to use field name **"picture"**:

```dart
// OLD (might not work)
request.files.add(await http.MultipartFile.fromPath('file', imagePath));

// NEW (correct)
request.files.add(await http.MultipartFile.fromPath('picture', imagePath));
```

