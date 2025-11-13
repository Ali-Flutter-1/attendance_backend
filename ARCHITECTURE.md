# Architecture Overview

This document explains the architecture and design decisions of the Attendance Management System API.

## Project Structure

```
attendance/
├── Controllers/          # API endpoints
│   ├── AttendanceController.cs
│   ├── LeaveController.cs
│   ├── ProfileController.cs
│   └── AdminController.cs
├── Data/                 # Database context
│   └── AttendanceDbContext.cs
├── DTOs/                 # Data Transfer Objects
│   ├── CheckInRequest.cs
│   ├── CheckOutRequest.cs
│   ├── LeaveRequest.cs
│   ├── AttendanceResponse.cs
│   └── ...
├── Models/               # Database entities
│   ├── User.cs
│   ├── Attendance.cs
│   ├── Leave.cs
│   └── OfficeLocation.cs
├── Services/             # Business logic
│   ├── LocationService.cs
│   ├── AttendanceService.cs
│   └── FileService.cs
├── Program.cs            # Application entry point
└── appsettings.json      # Configuration
```

## Architecture Layers

### 1. Controllers Layer (API Endpoints)
- **Purpose**: Handle HTTP requests and responses
- **Responsibilities**:
  - Validate input
  - Call services
  - Format responses
  - Handle errors

**Example Flow:**
```
HTTP Request → Controller → Service → Database → Service → Controller → HTTP Response
```

### 2. Services Layer (Business Logic)
- **Purpose**: Contains business rules and operations
- **Services**:
  - `LocationService`: GPS distance calculations
  - `AttendanceService`: Attendance rules and statistics
  - `FileService`: File upload/download operations

**Why Services?**
- Separates business logic from controllers
- Makes code reusable
- Easier to test
- Follows Single Responsibility Principle

### 3. Data Layer (Database)
- **Purpose**: Data persistence
- **Components**:
  - `DbContext`: Entity Framework context
  - `Models`: Database entities
  - Migrations: Database schema versioning

## Key Design Patterns

### 1. Repository Pattern (via DbContext)
- Entity Framework acts as a repository
- Abstracts database operations
- Easy to swap database providers

### 2. DTO Pattern
- Separates API contracts from database models
- Prevents exposing internal structure
- Allows versioning of APIs

### 3. Service Pattern
- Encapsulates business logic
- Promotes code reusability
- Makes testing easier

## Data Flow

### Check-In Flow
```
1. Flutter App → POST /api/attendance/checkin
   - Sends: userId, latitude, longitude, picture
   
2. AttendanceController.CheckIn()
   - Validates user exists
   - Checks if already checked in today
   
3. LocationService.IsWithinOfficeRange()
   - Calculates distance using Haversine formula
   - Returns true if within 50m
   
4. FileService.SaveAttendancePictureAsync()
   - Validates file type
   - Saves to wwwroot/uploads/
   - Returns file path
   
5. AttendanceService / DbContext
   - Creates/updates Attendance record
   - Sets IsPresent = true
   - Saves to database
   
6. Controller returns response
```

### Leave Application Flow
```
1. Flutter App → POST /api/leave/apply
   - Sends: userId, type, reason, startDate, endDate
   
2. LeaveController.ApplyForLeave()
   - Validates user exists
   - Validates dates
   - Validates leave type
   
3. Creates Leave record
   - Status = Pending
   - Saves to database
   
4. Admin reviews via GET /api/leave/pending
   
5. Admin approves/declines via POST /api/leave/approve
   - Updates Leave.Status
   - Adds AdminRemarks
```

## Database Design

### Relationships
```
User (1) ──→ (Many) Attendance
User (1) ──→ (Many) Leave
```

### Constraints
- **Unique Email**: Each user has unique email
- **One Attendance Per Day**: Unique constraint on (UserId, Date)
- **Cascade Delete**: Deleting user deletes their attendances and leaves

### Indexes
- Email (for fast user lookup)
- (UserId, Date) composite index (for attendance queries)
- CreatedAt (for sorting)

## Location Verification

### Haversine Formula
Calculates great-circle distance between two GPS coordinates.

**Formula:**
```
a = sin²(Δlat/2) + cos(lat1) × cos(lat2) × sin²(Δlon/2)
c = 2 × atan2(√a, √(1−a))
distance = R × c
```

Where:
- R = Earth's radius (6,371,000 meters)
- lat1, lon1 = Office coordinates
- lat2, lon2 = User coordinates

**Why Haversine?**
- Accounts for Earth's curvature
- Accurate for short distances (< 100km)
- Standard formula for GPS calculations

## File Storage Strategy

### Structure
```
wwwroot/
└── uploads/
    ├── profile-pictures/
    │   └── {userId}/
    │       └── {guid}.jpg
    └── attendance-pictures/
        └── {userId}/
            ├── checkin/
            │   └── {guid}.jpg
            └── checkout/
                └── {guid}.jpg
```

### Benefits
- Organized by user and type
- Easy to find files
- Can implement user-specific quotas
- Easy to clean up old files

### Future Improvements
- Cloud storage (Azure Blob, AWS S3)
- CDN for faster access
- Image compression
- Thumbnail generation

## Error Handling Strategy

### Response Format
```json
{
  "success": false,
  "message": "Error description",
  "data": null
}
```

### HTTP Status Codes
- `200 OK`: Success
- `400 Bad Request`: Invalid input
- `404 Not Found`: Resource not found
- `500 Internal Server Error`: Server error

### Exception Handling
- Try-catch in controllers
- Returns user-friendly messages
- Logs detailed errors (for debugging)

## Security Considerations

### Current State
- ⚠️ No authentication
- ⚠️ No authorization
- ⚠️ CORS allows all origins (development)

### Production Requirements
1. **JWT Authentication**
   - Token-based auth
   - Refresh tokens
   - Token expiration

2. **Authorization**
   - Role-based (Admin, User)
   - Resource-level permissions

3. **Input Validation**
   - Model validation
   - SQL injection prevention (EF Core handles this)
   - XSS prevention

4. **HTTPS Only**
   - Enforce SSL/TLS
   - Secure cookies

5. **Rate Limiting**
   - Prevent abuse
   - Limit requests per IP

## Performance Optimizations

### Database
- Indexes on frequently queried columns
- Eager loading with `.Include()`
- Pagination for large datasets

### Caching (Future)
- Cache office location
- Cache user profiles
- Cache monthly statistics

### Async/Await
- All database operations are async
- Non-blocking I/O
- Better scalability

## Testing Strategy

### Unit Tests (Future)
- Test services in isolation
- Mock dependencies
- Test business logic

### Integration Tests (Future)
- Test API endpoints
- Test database operations
- Test file uploads

### Manual Testing
- Use Swagger UI
- Use Postman
- Test with Flutter app

## Deployment Considerations

### Configuration
- Connection strings in `appsettings.json`
- Environment-specific settings
- Secrets management (Azure Key Vault)

### Database Migrations
- Run migrations on deployment
- Backup before migration
- Rollback strategy

### File Storage
- Ensure `wwwroot` folder exists
- Set proper permissions
- Consider cloud storage

## Future Enhancements

1. **Authentication & Authorization**
   - JWT tokens
   - Role-based access control

2. **Scheduled Jobs**
   - Mark absent users daily
   - Send attendance reminders
   - Generate monthly reports

3. **Notifications**
   - Email notifications
   - Push notifications
   - SMS alerts

4. **Reporting**
   - PDF reports
   - Excel exports
   - Dashboard analytics

5. **Advanced Features**
   - Multiple office locations
   - Shift management
   - Overtime tracking
   - Break time tracking

## Learning Points

### For Beginners

1. **Separation of Concerns**
   - Controllers handle HTTP
   - Services handle business logic
   - Models represent data

2. **Dependency Injection**
   - Services injected via constructor
   - Makes code testable
   - Loose coupling

3. **Entity Framework**
   - ORM (Object-Relational Mapping)
   - Code-first approach
   - Migrations for schema changes

4. **Async Programming**
   - `async`/`await` for I/O operations
   - Non-blocking calls
   - Better performance

5. **RESTful API Design**
   - Resource-based URLs
   - HTTP methods (GET, POST, PUT, DELETE)
   - Status codes

## Common Questions

**Q: Why use DTOs instead of Models directly?**
A: DTOs allow you to:
- Hide internal structure
- Version your API
- Add validation attributes
- Transform data

**Q: Why separate Services from Controllers?**
A: Services:
- Can be reused
- Are easier to test
- Keep controllers thin
- Follow Single Responsibility Principle

**Q: Why use async/await everywhere?**
A: Async operations:
- Don't block threads
- Improve scalability
- Better for I/O operations
- Modern best practice

**Q: How does Entity Framework work?**
A: EF Core:
- Maps C# classes to database tables
- Generates SQL automatically
- Tracks changes
- Handles relationships

## Conclusion

This architecture follows ASP.NET Core best practices:
- ✅ Separation of concerns
- ✅ Dependency injection
- ✅ Async/await patterns
- ✅ RESTful API design
- ✅ Clean code principles

The code is structured to be:
- **Maintainable**: Easy to modify
- **Testable**: Services can be mocked
- **Scalable**: Async operations
- **Learnable**: Clear structure

