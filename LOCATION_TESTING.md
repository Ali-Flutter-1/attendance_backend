# Location Testing Guide

## Your Office Coordinates

- **Latitude:** 31.413239
- **Longitude:** 73.0988347
- **Location:** Pakistan (Lahore area)
- **Allowed Radius:** 50 meters

## Verify Your Office Location

### Step 1: Check Current Office Location in Database

Use this API endpoint:
```http
GET /api/admin/office-location
```

This will show you the current office location stored in the database.

### Step 2: Update Office Location via API

If you need to update it:
```http
POST /api/admin/office-location
Content-Type: application/json

{
  "name": "Main Office",
  "latitude": 31.413239,
  "longitude": 73.0988347,
  "allowedRadiusInMeters": 50
}
```

### Step 3: Test Location Distance

The API calculates distance using the Haversine formula. To test if your device location is within range:

1. **Get your device's GPS coordinates** (from Flutter app)
2. **Send check-in request** with those coordinates
3. **API will calculate distance** and tell you if you're within 50 meters

## Common Issues

### Issue: Device Shows Wrong Location

**Possible Causes:**
1. **GPS not enabled** - Make sure GPS is enabled on your device
2. **Location services disabled** - Check device settings
3. **Using emulator** - Emulators often show default locations (like USA)
4. **WiFi location** - Using WiFi instead of GPS can give inaccurate results
5. **Indoor location** - GPS doesn't work well indoors

**Solutions:**
- Use a **physical device** (not emulator) for testing
- Enable **High Accuracy GPS** mode
- Go **outside** or near a window for better GPS signal
- Use **GPS only** mode (not WiFi/Network location)

### Issue: Coordinates Seem Wrong

**Verify coordinates:**
1. Open Google Maps
2. Search for: `31.413239, 73.0988347`
3. Check if it shows your office location

If it shows a different location, you need to update the coordinates.

### Issue: Always Getting "Too Far" Error

**Check:**
1. Is the office location set correctly in the database?
2. Are you sending the correct coordinates from Flutter?
3. Is the radius set correctly? (should be 50, not 500000000000!)

**Test:**
- Try increasing the radius temporarily to 100 meters for testing
- Check the distance returned in the error message
- Verify your device's actual GPS coordinates

## Testing Location in Flutter

### Get Device Location

```dart
Position position = await Geolocator.getCurrentPosition(
  desiredAccuracy: LocationAccuracy.high,  // Use high accuracy
);

print('Latitude: ${position.latitude}');
print('Longitude: ${position.longitude}');
```

### Test with Office Coordinates

For testing, you can temporarily use the office coordinates:

```dart
// Use office coordinates for testing
double testLat = 31.413239;
double testLon = 73.0988347;

// Send check-in with these coordinates
request.fields['latitude'] = testLat.toString();
request.fields['longitude'] = testLon.toString();
```

## Coordinate Format

- **Latitude:** -90 to +90 (North/South)
  - Positive = North of equator
  - Negative = South of equator
- **Longitude:** -180 to +180 (East/West)
  - Positive = East of prime meridian
  - Negative = West of prime meridian

**Your coordinates:**
- 31.413239 = North (Pakistan is north of equator)
- 73.0988347 = East (Pakistan is east of prime meridian)

## Quick Test

1. **Get office location from API:**
   ```bash
   GET /api/admin/office-location
   ```

2. **Use those exact coordinates in Flutter:**
   ```dart
   request.fields['latitude'] = '31.413239';
   request.fields['longitude'] = '73.0988347';
   ```

3. **This should work** (distance = 0 meters, within 50m range)

## If Your Device GPS is Wrong

If your laptop/device GPS is showing wrong location:

1. **Check device location settings**
2. **Use a phone** instead (phones have better GPS)
3. **Use Google Maps** to verify your actual location
4. **Update coordinates** in appsettings.json if needed

## Important Notes

- The radius is set to **50 meters** (not 500000000000!)
- Coordinates are in **decimal degrees** format
- The API uses **Haversine formula** for accurate distance calculation
- Distance is calculated in **meters**

