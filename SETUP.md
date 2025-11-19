# Step-by-Step Setup Guide

This guide will walk you through setting up the Attendance Management System API step by step.

## Step 1: Install Prerequisites

1. **Install .NET 8.0 SDK**
   - Download from: https://dotnet.microsoft.com/download/dotnet/8.0
   - Verify installation: `dotnet --version`

2. **Install SQL Server**
   - Option 1: SQL Server LocalDB (included with Visual Studio)
   - Option 2: SQL Server Express (free): https://www.microsoft.com/sql-server/sql-server-downloads
   - Verify installation: SQL Server should be running

## Step 2: Configure Database Connection

1. Open `appsettings.json`
2. Update the connection string:

   **For LocalDB (default):**
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=AttendanceDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
   }
   ```

   **For SQL Server Express:**
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=AttendanceDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
   }
   ```

   **For SQL Server (named instance):**
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=AttendanceDB;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True"
   }
   ```

## Step 3: Restore NuGet Packages

Open terminal in the project directory and run:

```bash
cd attendance
dotnet restore
```

This will download all required packages:
- Entity Framework Core
- SQL Server provider
- Swagger

## Step 4: Create Database Migration

1. **Create the initial migration:**
   ```bash
   dotnet ef migrations add InitialCreate
   ```

   This creates a `Migrations` folder with the database schema.

2. **Apply migration to create database:**
   ```bash
   dotnet ef database update
   ```

   This creates the `AttendanceDB` database with all tables.

## Step 5: Run the API

```bash
dotnet run
```

The API will start on:
- HTTP: `http://localhost:5201`
- HTTPS: `https://localhost:7225`
- Swagger: `https://localhost:7225/swagger`

## Step 6: Test the API

### Option 1: Using Swagger UI

1. Navigate to `https://localhost:7225/swagger`
2. Try the endpoints interactively

### Option 2: Using Postman or similar

Import the endpoints from the README.md file.

## Step 7: Create Sample Data

### Create a User (via SQL or API)

**Using SQL:**
```sql
USE AttendanceDB;
GO

INSERT INTO Users (FirstName, LastName, Email, Domain, IsAdmin, CreatedAt)
VALUES ('Admin', 'User', 'admin@company.com', 'IT', 1, GETUTCDATE());

INSERT INTO Users (FirstName, LastName, Email, Domain, IsAdmin, CreatedAt)
VALUES ('John', 'Doe', 'john@company.com', 'HR', 0, GETUTCDATE());
```

**Or use the API** (once you add a user creation endpoint or use Entity Framework directly).

### Set Office Location

Update `appsettings.json` with your office coordinates:

```json
{
  "OfficeLocation": {
    "Name": "Main Office",
    "Latitude": 31.413239,
    "Longitude": 73.0988347,
    "AllowedRadiusInMeters": 50
  }
}
```

The office location will be automatically initialized from `appsettings.json` when the API starts. Alternatively, you can update it directly in the database using SQL (see `UPDATE_OFFICE_LOCATION.sql`).

## Step 8: Test Check-In

1. Get your office GPS coordinates (use Google Maps)
2. Use Postman or Swagger to test check-in:

```http
POST https://localhost:7225/api/attendance/checkin
Content-Type: multipart/form-data

Form Data:
- UserId: 1
- Latitude: 31.413239
- Longitude: [your office longitude]
- Picture: [select an image file]
```

## Troubleshooting

### Issue: "Cannot connect to database"

**Solution:**
1. Check if SQL Server is running
2. Verify connection string in `appsettings.json`
3. Try connecting with SQL Server Management Studio

### Issue: "Migration failed"

**Solution:**
1. Delete the `Migrations` folder
2. Run `dotnet ef migrations add InitialCreate` again
3. If database exists, drop it first: `dotnet ef database drop`

### Issue: "CORS error in Flutter"

**Solution:**
1. Update CORS policy in `Program.cs` to include your Flutter app's URL
2. Make sure the API is running on HTTPS

### Issue: "File upload fails"

**Solution:**
1. Check that `wwwroot` folder exists
2. Ensure write permissions on the uploads folder
3. Check file size limits (default is 30MB)

## Next Steps

1. **Add Authentication**: Implement JWT tokens for secure API access
2. **Create User Registration**: Add endpoint to create new users
3. **Add Scheduled Jobs**: Automatically mark absent users daily
4. **Add Email Notifications**: Notify users about leave approvals

## Database Schema Overview

After migration, you'll have these tables:

- **Users**: User information
- **Attendances**: Check-in/out records
- **Leaves**: Leave applications
- **OfficeLocations**: Office GPS coordinates

All tables have proper relationships and indexes configured.

