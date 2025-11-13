# SQL Queries Reference

This document contains SQL queries for all database operations in the Attendance Management System.

## Table Names
- `Users` - User information
- `Attendances` - Check-in/out records
- `Leaves` - Leave applications
- `OfficeLocations` - Office GPS coordinates

---

## 👤 USER OPERATIONS

### 1. Create/Insert User (POST equivalent)

```sql
-- Insert a new user
INSERT INTO Users (FirstName, LastName, Email, Domain, Address, IsAdmin, CreatedAt)
VALUES ('John', 'Doe', 'john.doe@company.com', 'IT', '123 Main Street', 0, GETUTCDATE());

-- Insert admin user
INSERT INTO Users (FirstName, LastName, Email, Domain, Address, IsAdmin, CreatedAt)
VALUES ('Admin', 'User', 'admin@company.com', 'Management', 'Office Building', 1, GETUTCDATE());
```

### 2. Get All Users (GET equivalent)

```sql
-- Get all users
SELECT 
    Id,
    FirstName,
    LastName,
    Email,
    Domain,
    Address,
    ProfilePicturePath,
    IsAdmin,
    CreatedAt,
    UpdatedAt
FROM Users
ORDER BY FirstName, LastName;

-- Get user by ID
SELECT * FROM Users WHERE Id = 1;

-- Get user by Email
SELECT * FROM Users WHERE Email = 'john.doe@company.com';

-- Get all admin users
SELECT * FROM Users WHERE IsAdmin = 1;
```

### 3. Update User Profile (PUT equivalent)

```sql
-- Update user profile
UPDATE Users
SET 
    FirstName = 'John',
    LastName = 'Smith',
    Email = 'john.smith@company.com',
    Domain = 'HR',
    Address = '456 New Street',
    UpdatedAt = GETUTCDATE()
WHERE Id = 1;

-- Update profile picture path
UPDATE Users
SET 
    ProfilePicturePath = 'uploads/profile-pictures/1/abc123.jpg',
    UpdatedAt = GETUTCDATE()
WHERE Id = 1;
```

### 4. Delete User

```sql
-- Delete user (Note: This will cascade delete attendances and leaves)
DELETE FROM Users WHERE Id = 1;
```

---

## 📍 ATTENDANCE OPERATIONS

### 1. Create/Insert Check-In (POST /api/attendance/checkin)

```sql
-- Insert check-in record
INSERT INTO Attendances (
    UserId, 
    Date, 
    CheckInTime, 
    CheckInLatitude, 
    CheckInLongitude, 
    CheckInPicturePath, 
    IsPresent, 
    IsAbsent, 
    IsLateCheckIn,
    CreatedAt
)
VALUES (
    1,                              -- UserId
    CAST(GETUTCDATE() AS DATE),     -- Today's date
    GETUTCDATE(),                   -- Current UTC time
    40.741895,                      -- Latitude
    -73.989308,                     -- Longitude
    'uploads/attendance-pictures/1/checkin/abc123.jpg',
    1,                              -- IsPresent = true
    0,                              -- IsAbsent = false
    0,                              -- IsLateCheckIn (0 = on time, 1 = late)
    GETUTCDATE()
);
```

### 2. Update Check-Out (POST /api/attendance/checkout)

```sql
-- Update attendance with check-out
UPDATE Attendances
SET 
    CheckOutTime = GETUTCDATE(),
    CheckOutLatitude = 40.741895,
    CheckOutLongitude = -73.989308,
    CheckOutPicturePath = 'uploads/attendance-pictures/1/checkout/xyz789.jpg',
    IsEarlyCheckOut = 0             -- 0 = on time, 1 = early
WHERE UserId = 1 
  AND Date = CAST(GETUTCDATE() AS DATE);
```

### 3. Get Today's Attendance (GET /api/attendance/today/{userId})

```sql
-- Get today's attendance for a user
SELECT 
    a.Id,
    a.UserId,
    u.FirstName + ' ' + u.LastName AS UserName,
    a.Date,
    a.CheckInTime,
    a.CheckOutTime,
    a.CheckInPicturePath,
    a.CheckOutPicturePath,
    a.IsPresent,
    a.IsAbsent,
    a.IsLateCheckIn,
    a.IsEarlyCheckOut
FROM Attendances a
INNER JOIN Users u ON a.UserId = u.Id
WHERE a.UserId = 1 
  AND a.Date = CAST(GETUTCDATE() AS DATE);
```

### 4. Get All Attendances for a User

```sql
-- Get all attendance records for a user
SELECT 
    a.*,
    u.FirstName + ' ' + u.LastName AS UserName
FROM Attendances a
INNER JOIN Users u ON a.UserId = u.Id
WHERE a.UserId = 1
ORDER BY a.Date DESC;
```

### 5. Get Monthly Attendance (GET /api/attendance/monthly/{userId})

```sql
-- Get attendance for a specific month
SELECT 
    a.Id,
    a.UserId,
    u.FirstName + ' ' + u.LastName AS UserName,
    a.Date,
    a.CheckInTime,
    a.CheckOutTime,
    a.IsPresent,
    a.IsAbsent,
    a.IsLateCheckIn,
    a.IsEarlyCheckOut
FROM Attendances a
INNER JOIN Users u ON a.UserId = u.Id
WHERE a.UserId = 1
  AND YEAR(a.Date) = 2024
  AND MONTH(a.Date) = 1
ORDER BY a.Date;
```

### 6. Get Monthly Statistics

```sql
-- Get monthly attendance statistics
SELECT 
    COUNT(*) AS TotalDays,
    SUM(CASE WHEN IsPresent = 1 THEN 1 ELSE 0 END) AS TotalPresent,
    SUM(CASE WHEN IsAbsent = 1 THEN 1 ELSE 0 END) AS TotalAbsent,
    SUM(CASE WHEN IsLateCheckIn = 1 THEN 1 ELSE 0 END) AS TotalLateCheckIns,
    SUM(CASE WHEN IsEarlyCheckOut = 1 THEN 1 ELSE 0 END) AS TotalEarlyCheckOuts,
    CAST(SUM(CASE WHEN IsPresent = 1 THEN 1 ELSE 0 END) * 100.0 / COUNT(*) AS DECIMAL(5,2)) AS PresentPercentage,
    CAST(SUM(CASE WHEN IsAbsent = 1 THEN 1 ELSE 0 END) * 100.0 / COUNT(*) AS DECIMAL(5,2)) AS AbsentPercentage
FROM Attendances
WHERE UserId = 1
  AND YEAR(Date) = 2024
  AND MONTH(Date) = 1;
```

### 7. Get All User Activities (Admin View)

```sql
-- Get all activities with filters
SELECT 
    a.Id,
    a.UserId,
    u.FirstName + ' ' + u.LastName AS UserName,
    u.Domain,
    a.Date,
    a.CheckInTime,
    a.CheckOutTime,
    a.IsPresent,
    a.IsAbsent,
    a.IsLateCheckIn,
    a.IsEarlyCheckOut
FROM Attendances a
INNER JOIN Users u ON a.UserId = u.Id
WHERE a.Date >= '2024-01-01'  -- Start date filter
  AND a.Date <= '2024-01-31'  -- End date filter
  -- AND a.UserId = 1          -- Optional: Filter by user
ORDER BY a.Date DESC, u.FirstName;
```

### 8. Mark User as Absent

```sql
-- Mark user as absent for a specific date
INSERT INTO Attendances (
    UserId, 
    Date, 
    IsPresent, 
    IsAbsent, 
    CreatedAt
)
VALUES (
    1,
    '2024-01-15',
    0,  -- IsPresent = false
    1,  -- IsAbsent = true
    GETUTCDATE()
);

-- Or update existing record
UPDATE Attendances
SET 
    IsPresent = 0,
    IsAbsent = 1
WHERE UserId = 1 
  AND Date = '2024-01-15';
```

---

## 🏖️ LEAVE OPERATIONS

### 1. Apply for Leave (POST /api/leave/apply)

```sql
-- Insert leave application
-- LeaveType: 1=Sick, 2=Casual, 3=Annual, 4=Emergency, 5=Other
-- LeaveStatus: 1=Pending, 2=Approved, 3=Declined
INSERT INTO Leaves (
    UserId,
    Type,
    Reason,
    StartDate,
    EndDate,
    Status,
    CreatedAt
)
VALUES (
    1,                              -- UserId
    1,                              -- Type: Sick
    'Medical appointment',          -- Reason
    '2024-01-20',                   -- StartDate
    '2024-01-21',                   -- EndDate
    1,                              -- Status: Pending
    GETUTCDATE()
);
```

### 2. Get User Leaves (GET /api/leave/user/{userId})

```sql
-- Get all leaves for a user
SELECT 
    l.Id,
    l.UserId,
    u.FirstName + ' ' + u.LastName AS UserName,
    l.Type,
    CASE l.Type
        WHEN 1 THEN 'Sick'
        WHEN 2 THEN 'Casual'
        WHEN 3 THEN 'Annual'
        WHEN 4 THEN 'Emergency'
        WHEN 5 THEN 'Other'
    END AS TypeName,
    l.Reason,
    l.StartDate,
    l.EndDate,
    l.Status,
    CASE l.Status
        WHEN 1 THEN 'Pending'
        WHEN 2 THEN 'Approved'
        WHEN 3 THEN 'Declined'
    END AS StatusName,
    l.AdminRemarks,
    l.CreatedAt,
    l.UpdatedAt
FROM Leaves l
INNER JOIN Users u ON l.UserId = u.Id
WHERE l.UserId = 1
ORDER BY l.CreatedAt DESC;
```

### 3. Get Pending Leaves (GET /api/leave/pending)

```sql
-- Get all pending leaves
SELECT 
    l.*,
    u.FirstName + ' ' + u.LastName AS UserName,
    u.Email
FROM Leaves l
INNER JOIN Users u ON l.UserId = u.Id
WHERE l.Status = 1  -- Pending
ORDER BY l.CreatedAt DESC;
```

### 4. Approve Leave (POST /api/leave/approve)

```sql
-- Approve a leave
UPDATE Leaves
SET 
    Status = 2,  -- Approved
    AdminRemarks = 'Approved',
    UpdatedAt = GETUTCDATE()
WHERE Id = 1;

-- Decline a leave
UPDATE Leaves
SET 
    Status = 3,  -- Declined
    AdminRemarks = 'Not enough leave balance',
    UpdatedAt = GETUTCDATE()
WHERE Id = 1;
```

### 5. Get Leaves by Status

```sql
-- Get approved leaves
SELECT * FROM Leaves WHERE Status = 2;

-- Get declined leaves
SELECT * FROM Leaves WHERE Status = 3;

-- Get leaves for a date range
SELECT * FROM Leaves
WHERE StartDate <= '2024-01-31'
  AND EndDate >= '2024-01-01';
```

---

## 📍 OFFICE LOCATION OPERATIONS

### 1. Set Office Location (POST /api/admin/office-location)

```sql
-- Insert office location
INSERT INTO OfficeLocations (
    Name,
    Latitude,
    Longitude,
    AllowedRadiusInMeters,
    IsActive,
    CreatedAt
)
VALUES (
    'Main Office',
    40.741895,
    -73.989308,
    50.0,
    1,  -- IsActive = true
    GETUTCDATE()
);

-- Deactivate all existing locations and set new one
UPDATE OfficeLocations SET IsActive = 0;

INSERT INTO OfficeLocations (
    Name,
    Latitude,
    Longitude,
    AllowedRadiusInMeters,
    IsActive,
    CreatedAt
)
VALUES (
    'New Office Location',
    40.750000,
    -74.000000,
    50.0,
    1,
    GETUTCDATE()
);
```

### 2. Get Office Location (GET /api/admin/office-location)

```sql
-- Get active office location
SELECT 
    Id,
    Name,
    Latitude,
    Longitude,
    AllowedRadiusInMeters,
    IsActive,
    CreatedAt
FROM OfficeLocations
WHERE IsActive = 1;

-- Get all office locations
SELECT * FROM OfficeLocations ORDER BY CreatedAt DESC;
```

### 3. Update Office Location

```sql
-- Update office location
UPDATE OfficeLocations
SET 
    Name = 'Updated Office Name',
    Latitude = 40.760000,
    Longitude = -74.010000,
    AllowedRadiusInMeters = 75.0
WHERE Id = 1;
```

---

## 📊 REPORTING QUERIES

### 1. Daily Attendance Report

```sql
-- Get daily attendance summary
SELECT 
    a.Date,
    COUNT(*) AS TotalEmployees,
    SUM(CASE WHEN a.IsPresent = 1 THEN 1 ELSE 0 END) AS PresentCount,
    SUM(CASE WHEN a.IsAbsent = 1 THEN 1 ELSE 0 END) AS AbsentCount,
    SUM(CASE WHEN a.IsLateCheckIn = 1 THEN 1 ELSE 0 END) AS LateCheckIns,
    SUM(CASE WHEN a.IsEarlyCheckOut = 1 THEN 1 ELSE 0 END) AS EarlyCheckOuts
FROM Attendances a
WHERE a.Date = CAST(GETUTCDATE() AS DATE)
GROUP BY a.Date;
```

### 2. User Attendance Summary

```sql
-- Get attendance summary for all users
SELECT 
    u.Id,
    u.FirstName + ' ' + u.LastName AS UserName,
    u.Domain,
    COUNT(a.Id) AS TotalDays,
    SUM(CASE WHEN a.IsPresent = 1 THEN 1 ELSE 0 END) AS PresentDays,
    SUM(CASE WHEN a.IsAbsent = 1 THEN 1 ELSE 0 END) AS AbsentDays,
    SUM(CASE WHEN a.IsLateCheckIn = 1 THEN 1 ELSE 0 END) AS LateCheckIns,
    SUM(CASE WHEN a.IsEarlyCheckOut = 1 THEN 1 ELSE 0 END) AS EarlyCheckOuts
FROM Users u
LEFT JOIN Attendances a ON u.Id = a.UserId
WHERE a.Date >= DATEADD(MONTH, -1, GETUTCDATE())  -- Last month
GROUP BY u.Id, u.FirstName, u.LastName, u.Domain
ORDER BY u.FirstName;
```

### 3. Leave Statistics

```sql
-- Get leave statistics by type
SELECT 
    CASE Type
        WHEN 1 THEN 'Sick'
        WHEN 2 THEN 'Casual'
        WHEN 3 THEN 'Annual'
        WHEN 4 THEN 'Emergency'
        WHEN 5 THEN 'Other'
    END AS LeaveType,
    COUNT(*) AS TotalLeaves,
    SUM(CASE WHEN Status = 2 THEN 1 ELSE 0 END) AS Approved,
    SUM(CASE WHEN Status = 3 THEN 1 ELSE 0 END) AS Declined,
    SUM(CASE WHEN Status = 1 THEN 1 ELSE 0 END) AS Pending
FROM Leaves
GROUP BY Type;
```

### 4. Users on Leave Today

```sql
-- Get users who are on approved leave today
SELECT 
    u.Id,
    u.FirstName + ' ' + u.LastName AS UserName,
    u.Email,
    l.Type,
    l.StartDate,
    l.EndDate,
    l.Reason
FROM Users u
INNER JOIN Leaves l ON u.Id = l.UserId
WHERE l.Status = 2  -- Approved
  AND CAST(GETUTCDATE() AS DATE) >= l.StartDate
  AND CAST(GETUTCDATE() AS DATE) <= l.EndDate;
```

### 5. Late Check-In Report

```sql
-- Get all late check-ins for a month
SELECT 
    a.Date,
    u.FirstName + ' ' + u.LastName AS UserName,
    u.Domain,
    a.CheckInTime,
    CAST(a.CheckInTime AS TIME) AS CheckInTimeOnly
FROM Attendances a
INNER JOIN Users u ON a.UserId = u.Id
WHERE a.IsLateCheckIn = 1
  AND YEAR(a.Date) = 2024
  AND MONTH(a.Date) = 1
ORDER BY a.Date, a.CheckInTime;
```

---

## 🔧 UTILITY QUERIES

### 1. Check Database Connection

```sql
SELECT @@VERSION AS SQLServerVersion;
SELECT GETDATE() AS CurrentDateTime;
SELECT GETUTCDATE() AS CurrentUTCDateTime;
```

### 2. Count Records

```sql
-- Count users
SELECT COUNT(*) AS TotalUsers FROM Users;

-- Count attendances
SELECT COUNT(*) AS TotalAttendances FROM Attendances;

-- Count leaves
SELECT COUNT(*) AS TotalLeaves FROM Leaves;
```

### 3. Delete Test Data

```sql
-- Delete test attendances
DELETE FROM Attendances WHERE UserId = 1 AND Date < '2024-01-01';

-- Delete test leaves
DELETE FROM Leaves WHERE UserId = 1 AND Status = 1;  -- Delete pending test leaves
```

### 4. Reset User Attendance for Today

```sql
-- Delete today's attendance for a user (to allow re-check-in for testing)
DELETE FROM Attendances 
WHERE UserId = 1 
  AND Date = CAST(GETUTCDATE() AS DATE);
```

### 5. Get Users Without Attendance Today

```sql
-- Get users who haven't checked in today
SELECT 
    u.Id,
    u.FirstName + ' ' + u.LastName AS UserName,
    u.Email
FROM Users u
LEFT JOIN Attendances a ON u.Id = a.UserId 
    AND a.Date = CAST(GETUTCDATE() AS DATE)
WHERE a.Id IS NULL
  AND u.IsAdmin = 0;  -- Exclude admins if needed
```

---

## 📝 NOTES

### Date/Time Functions
- `GETUTCDATE()` - Current UTC date and time
- `GETDATE()` - Current local date and time
- `CAST(GETUTCDATE() AS DATE)` - Today's date only

### Leave Type Values
- `1` = Sick
- `2` = Casual
- `3` = Annual
- `4` = Emergency
- `5` = Other

### Leave Status Values
- `1` = Pending
- `2` = Approved
- `3` = Declined

### Boolean Values
- `1` = true
- `0` = false

---

## 🚀 Quick Examples

### Complete Check-In Flow
```sql
-- 1. Check if user exists
SELECT * FROM Users WHERE Id = 1;

-- 2. Check if already checked in today
SELECT * FROM Attendances 
WHERE UserId = 1 AND Date = CAST(GETUTCDATE() AS DATE);

-- 3. Insert check-in
INSERT INTO Attendances (UserId, Date, CheckInTime, CheckInLatitude, CheckInLongitude, CheckInPicturePath, IsPresent, IsAbsent, IsLateCheckIn, CreatedAt)
VALUES (1, CAST(GETUTCDATE() AS DATE), GETUTCDATE(), 40.741895, -73.989308, 'uploads/attendance-pictures/1/checkin/pic.jpg', 1, 0, 0, GETUTCDATE());

-- 4. Verify check-in
SELECT * FROM Attendances WHERE UserId = 1 AND Date = CAST(GETUTCDATE() AS DATE);
```

### Complete Leave Application Flow
```sql
-- 1. Apply for leave
INSERT INTO Leaves (UserId, Type, Reason, StartDate, EndDate, Status, CreatedAt)
VALUES (1, 1, 'Medical appointment', '2024-01-20', '2024-01-21', 1, GETUTCDATE());

-- 2. Admin views pending leaves
SELECT l.*, u.FirstName + ' ' + u.LastName AS UserName 
FROM Leaves l 
INNER JOIN Users u ON l.UserId = u.Id 
WHERE l.Status = 1;

-- 3. Admin approves leave
UPDATE Leaves 
SET Status = 2, AdminRemarks = 'Approved', UpdatedAt = GETUTCDATE() 
WHERE Id = 1;
```

---

These SQL queries can be executed directly in SQL Server Management Studio (SSMS) or any SQL client tool.

