# Attendance Management System API

A comprehensive ASP.NET Core Web API for managing employee attendance with location verification, leave management, and admin features.

## Features

### User Features
- ✅ **Check-in/Check-out** with live picture upload
- ✅ **Location verification** (must be within 50m of office)
- ✅ **One check-in and one check-out per day** enforcement
- ✅ **Profile management** (update name, email, domain, profile picture)
- ✅ **Leave application** (type, reason, start/end date)
- ✅ **Monthly attendance view** with statistics
- ✅ **Present/Absent percentage** calculation

### Admin Features
- ✅ **View all users** (name, domain, address)
- ✅ **View all user activities** (attendance records)
- ✅ **Approve/Decline leave applications**
- ✅ **Filter activities** by date range and user
- ✅ **Configure office location** and allowed radius

## Prerequisites

- .NET 8.0 SDK
- SQL Server (LocalDB or SQL Server Express)
- Visual Studio 2022 or VS Code

## Setup Instructions

### Step 1: Update Connection String

Open `appsettings.json` and update the connection string to match your SQL Server:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER;Database=AttendanceDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
}
```

For LocalDB (default):
```
Server=(localdb)\\mssqllocaldb;Database=AttendanceDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True
```

### Step 2: Install Packages

Restore NuGet packages:
```bash
dotnet restore
```

### Step 3: Create Database

Run Entity Framework migrations to create the database:

```bash
# Create initial migration
dotnet ef migrations add InitialCreate

# Apply migration to database
dotnet ef database update
```

### Step 4: Configure Office Location

Before users can check in, you need to set the office location. Use the Admin API:

```http
POST /api/admin/office-location
Content-Type: application/json

{
  "name": "Main Office",
  "latitude": 40.741895,  // Your office latitude
  "longitude": -73.989308,  // Your office longitude
  "allowedRadiusInMeters": 50
}
```

### Step 5: Run the API

```bash
dotnet run
```

The API will be available at:
- HTTP: `http://localhost:5201`
- HTTPS: `https://localhost:7225`
- Swagger UI: `https://localhost:7225/swagger` (in development)

## API Endpoints

### Attendance Endpoints

#### Check In
```http
POST /api/attendance/checkin
Content-Type: multipart/form-data

Form Data:
- UserId: 1
- Latitude: 40.7128
- Longitude: -74.0060
- Picture: [image file]
```

#### Check Out
```http
POST /api/attendance/checkout
Content-Type: multipart/form-data

Form Data:
- UserId: 1
- Latitude: 40.7128
- Longitude: -74.0060
- Picture: [image file]
```

#### Get Today's Attendance
```http
GET /api/attendance/today/{userId}
```

#### Get Monthly Attendance
```http
GET /api/attendance/monthly/{userId}?year=2024&month=1
```

### Leave Endpoints

#### Apply for Leave
```http
POST /api/leave/apply
Content-Type: application/json

{
  "userId": 1,
  "type": 1,  // 1=Sick, 2=Casual, 3=Annual, 4=Emergency, 5=Other
  "reason": "Medical appointment",
  "startDate": "2024-01-15",
  "endDate": "2024-01-16"
}
```

#### Get User Leaves
```http
GET /api/leave/user/{userId}
```

#### Get Pending Leaves (Admin)
```http
GET /api/leave/pending
```

#### Approve/Decline Leave (Admin)
```http
POST /api/leave/approve
Content-Type: application/json

{
  "leaveId": 1,
  "status": 2,  // 2=Approved, 3=Declined
  "adminRemarks": "Approved"
}
```

### Profile Endpoints

#### Get Profile
```http
GET /api/profile/{userId}
```

#### Update Profile
```http
PUT /api/profile/update
Content-Type: multipart/form-data

Form Data:
- UserId: 1
- FirstName: John (optional)
- LastName: Doe (optional)
- Email: john@example.com (optional)
- Domain: IT (optional)
- Picture: [image file] (optional)
```

### Admin Endpoints

#### Get All Users
```http
GET /api/admin/users
```

#### Get User Activities
```http
GET /api/admin/activities?startDate=2024-01-01&endDate=2024-01-31&userId=1
```

#### Get User Statistics
```http
GET /api/admin/statistics/{userId}?year=2024&month=1
```

#### Set Office Location
```http
POST /api/admin/office-location
Content-Type: application/json

{
  "name": "Main Office",
  "latitude": 40.7128,
  "longitude": -74.0060,
  "allowedRadiusInMeters": 50
}
```

## Database Schema

### Users Table
- Id (Primary Key)
- FirstName
- LastName
- Email (Unique)
- Domain
- Address
- ProfilePicturePath
- IsAdmin
- CreatedAt
- UpdatedAt

### Attendances Table
- Id (Primary Key)
- UserId (Foreign Key)
- Date
- CheckInTime
- CheckOutTime
- CheckInPicturePath
- CheckOutPicturePath
- CheckInLatitude
- CheckInLongitude
- CheckOutLatitude
- CheckOutLongitude
- IsPresent
- IsAbsent
- CreatedAt

### Leaves Table
- Id (Primary Key)
- UserId (Foreign Key)
- Type (Enum: Sick, Casual, Annual, Emergency, Other)
- Reason
- StartDate
- EndDate
- Status (Enum: Pending, Approved, Declined)
- AdminRemarks
- CreatedAt
- UpdatedAt

### OfficeLocations Table
- Id (Primary Key)
- Name
- Latitude
- Longitude
- AllowedRadiusInMeters
- IsActive
- CreatedAt

## Location Calculation

The API uses the **Haversine formula** to calculate the distance between the user's location and the office location. The distance is calculated in meters.

Formula:
```
a = sin²(Δlat/2) + cos(lat1) × cos(lat2) × sin²(Δlon/2)
c = 2 × atan2(√a, √(1−a))
distance = R × c
```
Where R = Earth's radius (6,371,000 meters)

## File Storage

Uploaded files (profile pictures and attendance pictures) are stored in:
- `wwwroot/uploads/profile-pictures/{userId}/`
- `wwwroot/uploads/attendance-pictures/{userId}/checkin/`
- `wwwroot/uploads/attendance-pictures/{userId}/checkout/`

## Response Format

All API responses follow this format:

```json
{
  "success": true,
  "message": "Operation successful",
  "data": { ... }
}
```

## Error Handling

The API returns appropriate HTTP status codes:
- `200 OK` - Success
- `400 Bad Request` - Invalid request data
- `404 Not Found` - Resource not found
- `500 Internal Server Error` - Server error

## Testing with Flutter

When integrating with Flutter, make sure to:

1. **Update CORS settings** in `Program.cs` to allow your Flutter app's origin
2. **Use multipart/form-data** for file uploads
3. **Include GPS coordinates** from device location services
4. **Handle the response format** as shown above

Example Flutter HTTP request for check-in:

```dart
var request = http.MultipartRequest(
  'POST',
  Uri.parse('https://your-api-url/api/attendance/checkin'),
);

request.fields['userId'] = '1';
request.fields['latitude'] = '40.7128';
request.fields['longitude'] = '-74.0060';
request.files.add(await http.MultipartFile.fromPath('picture', imagePath));

var response = await request.send();
```

## Next Steps

- Add authentication and authorization (JWT tokens)
- Add password hashing for user accounts
- Implement scheduled job to mark absent users daily
- Add email notifications for leave approvals
- Add push notifications for attendance reminders

## Notes

- The API currently doesn't have authentication. You'll need to add it for production use.
- Office location must be configured before users can check in.
- Users can only check in/out once per day.
- Location verification requires users to be within the configured radius (default 50m).

