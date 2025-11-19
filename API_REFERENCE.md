# API Quick Reference Guide

## Base URL
- Development: `https://localhost:7225` or `http://localhost:5201`
- Production: Replace with your server URL

## Response Format
All endpoints return JSON in this format:
```json
{
  "success": true/false,
  "message": "Status message",
  "data": { ... }
}
```

---

## 📍 Attendance Endpoints

### 1. Check In
**POST** `/api/attendance/checkin`

**Content-Type:** `multipart/form-data`

**Form Fields:**
- `userId` (int, required)
- `latitude` (double, required)
- `longitude` (double, required)
- `picture` (file, required) - Image file

**Response:**
```json
{
  "success": true,
  "message": "Check-in successful",
  "data": {
    "id": 1,
    "userId": 1,
    "userName": "John Doe",
    "date": "2024-01-15T00:00:00",
    "checkInTime": "2024-01-15T09:00:00",
    "checkOutTime": null,
    "checkInPicturePath": "uploads/attendance-pictures/1/checkin/...",
    "isPresent": true,
    "isAbsent": false
  }
}
```

**Errors:**
- `400` - Already checked in today
- `400` - Too far from office (>50m)
- `400` - Picture required

---

### 2. Check Out
**POST** `/api/attendance/checkout`

**Content-Type:** `multipart/form-data`

**Form Fields:**
- `userId` (int, required)
- `latitude` (double, required)
- `longitude` (double, required)
- `picture` (file, required) - Image file

**Response:** Same format as check-in

**Errors:**
- `400` - Already checked out today
- `400` - Must check in first
- `400` - Too far from office

---

### 3. Get Today's Attendance
**GET** `/api/attendance/today/{userId}`

**Response:**
```json
{
  "success": true,
  "message": "Attendance retrieved successfully",
  "data": {
    "id": 1,
    "userId": 1,
    "userName": "John Doe",
    "date": "2024-01-15T00:00:00",
    "checkInTime": "2024-01-15T09:00:00",
    "checkOutTime": "2024-01-15T17:00:00",
    "checkInPicturePath": "...",
    "checkOutPicturePath": "...",
    "isPresent": true,
    "isAbsent": false
  }
}
```

---

### 4. Get Monthly Attendance
**GET** `/api/attendance/monthly/{userId}?year=2024&month=1`

**Query Parameters:**
- `year` (int, optional) - Default: current year
- `month` (int, optional) - Default: current month

**Response:**
```json
{
  "success": true,
  "message": "Monthly attendance retrieved successfully",
  "data": {
    "userId": 1,
    "userName": "John Doe",
    "year": 2024,
    "month": 1,
    "totalPresent": 20,
    "totalAbsent": 2,
    "presentPercentage": 90.91,
    "absentPercentage": 9.09,
    "dailyAttendances": [
      {
        "id": 1,
        "userId": 1,
        "userName": "John Doe",
        "date": "2024-01-01T00:00:00",
        "checkInTime": "2024-01-01T09:00:00",
        "checkOutTime": "2024-01-01T17:00:00",
        "isPresent": true,
        "isAbsent": false
      }
    ]
  }
}
```

---

## 🏖️ Leave Endpoints

### 1. Apply for Leave
**POST** `/api/leave/apply`

**Content-Type:** `application/json`

**Body:**
```json
{
  "userId": 1,
  "type": 1,  // 1=Sick, 2=Casual, 3=Annual, 4=Emergency, 5=Other
  "reason": "Medical appointment",
  "startDate": "2024-01-20",
  "endDate": "2024-01-21"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Leave application submitted successfully",
  "data": {
    "id": 1,
    "userId": 1,
    "userName": "John Doe",
    "type": "Sick",
    "reason": "Medical appointment",
    "startDate": "2024-01-20T00:00:00",
    "endDate": "2024-01-21T00:00:00",
    "status": "Pending",
    "createdAt": "2024-01-15T10:00:00"
  }
}
```

---

### 2. Get User Leaves
**GET** `/api/leave/user/{userId}`

**Response:**
```json
{
  "success": true,
  "message": "Leaves retrieved successfully",
  "data": [
    {
      "id": 1,
      "userId": 1,
      "userName": "John Doe",
      "type": "Sick",
      "reason": "Medical appointment",
      "startDate": "2024-01-20T00:00:00",
      "endDate": "2024-01-21T00:00:00",
      "status": "Approved",
      "adminRemarks": "Approved",
      "createdAt": "2024-01-15T10:00:00"
    }
  ]
}
```

---

### 3. Get Pending Leaves (Admin)
**GET** `/api/leave/pending`

**Response:** Same format as Get User Leaves

---

### 4. Approve/Decline Leave (Admin)
**POST** `/api/leave/approve`

**Content-Type:** `application/json`

**Body:**
```json
{
  "leaveId": 1,
  "status": 2,  // 2=Approved, 3=Declined
  "adminRemarks": "Approved"
}
```

**Response:** Leave object with updated status

---

## 👤 Profile Endpoints

### 1. Get Profile
**GET** `/api/profile/{userId}`

**Response:**
```json
{
  "success": true,
  "message": "Profile retrieved successfully",
  "data": {
    "id": 1,
    "firstName": "John",
    "lastName": "Doe",
    "email": "john@company.com",
    "domain": "IT",
    "address": "123 Main St",
    "profilePicturePath": "uploads/profile-pictures/1/...",
    "isAdmin": false
  }
}
```

---

### 2. Update Profile
**PUT** `/api/profile/update`

**Content-Type:** `multipart/form-data`

**Form Fields (all optional):**
- `userId` (int, required)
- `firstName` (string, optional)
- `lastName` (string, optional)
- `email` (string, optional)
- `domain` (string, optional)
- `picture` (file, optional) - New profile picture

**Response:** Updated user profile

---

## 👨‍💼 Admin Endpoints

### 1. Create User
**POST** `/api/admin/users`

**Content-Type:** `application/json`

**Body:**
```json
{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john@company.com",
  "domain": "IT",
  "address": "123 Main St",
  "isAdmin": false
}
```

**Response:** Created user object

---

### 2. Get All Users
**GET** `/api/admin/users`

**Response:**
```json
{
  "success": true,
  "message": "Users retrieved successfully",
  "data": [
    {
      "id": 1,
      "firstName": "John",
      "lastName": "Doe",
      "email": "john@company.com",
      "domain": "IT",
      "address": "123 Main St",
      "profilePicturePath": "...",
      "isAdmin": false
    }
  ]
}
```

---

### 3. Get User Activities
**GET** `/api/admin/activities?startDate=2024-01-01&endDate=2024-01-31&userId=1`

**Query Parameters:**
- `startDate` (date, optional) - Default: 30 days ago
- `endDate` (date, optional) - Default: today
- `userId` (int, optional) - Filter by user

**Response:** List of attendance records

---

### 4. Get User Statistics
**GET** `/api/admin/statistics/{userId}?year=2024&month=1`

**Query Parameters:**
- `year` (int, optional) - Default: current year
- `month` (int, optional) - Default: current month

**Response:** Monthly attendance statistics (same format as monthly attendance endpoint)

---

### 5. Get Office Location
**GET** `/api/admin/office-location`

**Response:**
```json
{
  "success": true,
  "message": "Office location retrieved successfully",
  "data": {
    "id": 1,
    "name": "Main Office",
    "latitude": 31.413239,
    "longitude": 73.0988347,
    "allowedRadiusInMeters": 50,
    "isActive": true
  }
}
```

**Note:** Office location is configured via `appsettings.json` or directly in the database using SQL. There is no POST endpoint for setting office location.

---

## 📝 Notes

### Leave Types
- `1` = Sick
- `2` = Casual
- `3` = Annual
- `4` = Emergency
- `5` = Other

### Leave Status
- `1` = Pending
- `2` = Approved
- `3` = Declined

### Date Format
- Use ISO 8601 format: `YYYY-MM-DD` or `YYYY-MM-DDTHH:mm:ss`
- Dates are stored in UTC

### File Uploads
- Maximum file size: 30MB (default)
- Supported formats: JPG, JPEG, PNG, GIF
- Files are stored in `wwwroot/uploads/`

### Location Requirements
- Users must be within the configured radius (default: 50 meters) to check in/out
- Distance is calculated using the Haversine formula
- Office location must be configured before users can check in

---

## 🔒 Security Notes

⚠️ **Important:** This API currently does NOT have authentication. For production:
1. Add JWT token authentication
2. Implement role-based authorization
3. Add rate limiting
4. Restrict CORS to specific origins
5. Add input validation and sanitization
6. Implement HTTPS only

---

## 🧪 Testing Examples

### Using cURL

**Check In:**
```bash
curl -X POST "https://localhost:7225/api/attendance/checkin" \
  -F "userId=1" \
  -F "latitude=31.413239" \
  -F "longitude=-74.0060" \
  -F "picture=@/path/to/image.jpg"
```

**Apply for Leave:**
```bash
curl -X POST "https://localhost:7225/api/leave/apply" \
  -H "Content-Type: application/json" \
  -d '{
    "userId": 1,
    "type": 1,
    "reason": "Medical appointment",
    "startDate": "2024-01-20",
    "endDate": "2024-01-21"
  }'
```

### Using Flutter (Dart)

```dart
import 'package:http/http.dart' as http;
import 'dart:io';

Future<void> checkIn(int userId, double lat, double lon, File image) async {
  var request = http.MultipartRequest(
    'POST',
    Uri.parse('https://your-api-url/api/attendance/checkin'),
  );
  
  request.fields['userId'] = userId.toString();
  request.fields['latitude'] = lat.toString();
  request.fields['longitude'] = lon.toString();
  request.files.add(await http.MultipartFile.fromPath('picture', image.path));
  
  var response = await request.send();
  var responseData = await response.stream.bytesToString();
  print(responseData);
}
```

