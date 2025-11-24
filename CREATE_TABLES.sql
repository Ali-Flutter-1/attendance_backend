-- =============================================
-- Attendance Management System - Database Schema
-- Create all tables manually (without migrations)
-- =============================================

USE ATTENDANCE;  -- Replace with your database name if different
GO

-- =============================================
-- 1. USERS TABLE
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Users] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [FirstName] NVARCHAR(50) NOT NULL,
        [LastName] NVARCHAR(50) NOT NULL,
        [Email] NVARCHAR(100) NOT NULL,
        [PasswordHash] NVARCHAR(256) NULL,
        [Domain] NVARCHAR(100) NULL,
        [Address] NVARCHAR(500) NULL,
        [ProfilePicturePath] NVARCHAR(500) NULL,
        [IsAdmin] BIT NOT NULL DEFAULT 0,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NULL,
        
        CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
    
    -- Create unique index on Email
    CREATE UNIQUE NONCLUSTERED INDEX [IX_Users_Email] 
    ON [dbo].[Users] ([Email] ASC);
    
    PRINT 'Users table created successfully.';
END
ELSE
BEGIN
    PRINT 'Users table already exists.';
END
GO

-- =============================================
-- 2. OFFICE LOCATIONS TABLE
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[OfficeLocations]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[OfficeLocations] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [Name] NVARCHAR(200) NOT NULL DEFAULT 'Main Office',
        [Latitude] FLOAT NOT NULL,
        [Longitude] FLOAT NOT NULL,
        [AllowedRadiusInMeters] FLOAT NOT NULL DEFAULT 50.0,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        CONSTRAINT [PK_OfficeLocations] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
    
    PRINT 'OfficeLocations table created successfully.';
END
ELSE
BEGIN
    PRINT 'OfficeLocations table already exists.';
END
GO

-- =============================================
-- 3. ATTENDANCES TABLE
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Attendances]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Attendances] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [UserId] INT NOT NULL,
        [Date] DATE NOT NULL,
        [CheckInTime] DATETIME2 NULL,
        [CheckOutTime] DATETIME2 NULL,
        [CheckInPicturePath] NVARCHAR(500) NULL,
        [CheckOutPicturePath] NVARCHAR(500) NULL,
        [CheckInLatitude] FLOAT NULL,
        [CheckInLongitude] FLOAT NULL,
        [CheckOutLatitude] FLOAT NULL,
        [CheckOutLongitude] FLOAT NULL,
        [IsPresent] BIT NOT NULL DEFAULT 0,
        [IsAbsent] BIT NOT NULL DEFAULT 0,
        [IsLateCheckIn] BIT NOT NULL DEFAULT 0,
        [IsEarlyCheckOut] BIT NOT NULL DEFAULT 0,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        CONSTRAINT [PK_Attendances] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_Attendances_Users_UserId] FOREIGN KEY ([UserId]) 
            REFERENCES [dbo].[Users] ([Id]) 
            ON DELETE CASCADE
    );
    
    -- Create unique index on UserId and Date (one attendance per user per day)
    CREATE UNIQUE NONCLUSTERED INDEX [IX_Attendances_UserId_Date] 
    ON [dbo].[Attendances] ([UserId] ASC, [Date] ASC);
    
    -- Create index on UserId for faster lookups
    CREATE NONCLUSTERED INDEX [IX_Attendances_UserId] 
    ON [dbo].[Attendances] ([UserId] ASC);
    
    PRINT 'Attendances table created successfully.';
END
ELSE
BEGIN
    PRINT 'Attendances table already exists.';
END
GO

-- =============================================
-- 4. LEAVES TABLE
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Leaves]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Leaves] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [UserId] INT NOT NULL,
        [Type] INT NOT NULL,  -- 1=Sick, 2=Casual, 3=Annual, 4=Emergency, 5=Other
        [Reason] NVARCHAR(1000) NOT NULL,
        [StartDate] DATE NOT NULL,
        [EndDate] DATE NOT NULL,
        [Status] INT NOT NULL DEFAULT 1,  -- 1=Pending, 2=Approved, 3=Declined
        [AdminRemarks] NVARCHAR(500) NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NULL,
        
        CONSTRAINT [PK_Leaves] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_Leaves_Users_UserId] FOREIGN KEY ([UserId]) 
            REFERENCES [dbo].[Users] ([Id]) 
            ON DELETE CASCADE
    );
    
    -- Create index on UserId for faster lookups
    CREATE NONCLUSTERED INDEX [IX_Leaves_UserId] 
    ON [dbo].[Leaves] ([UserId] ASC);
    
    -- Create index on Status for filtering pending leaves
    CREATE NONCLUSTERED INDEX [IX_Leaves_Status] 
    ON [dbo].[Leaves] ([Status] ASC);
    
    PRINT 'Leaves table created successfully.';
END
ELSE
BEGIN
    PRINT 'Leaves table already exists.';
END
GO

-- =============================================
-- VERIFICATION
-- =============================================
PRINT '';
PRINT '========================================';
PRINT 'Database Schema Creation Complete!';
PRINT '========================================';
PRINT '';
PRINT 'Tables created:';
PRINT '  1. Users (Primary Key: Id)';
PRINT '  2. OfficeLocations (Primary Key: Id)';
PRINT '  3. Attendances (Primary Key: Id, Foreign Key: UserId -> Users.Id)';
PRINT '  4. Leaves (Primary Key: Id, Foreign Key: UserId -> Users.Id)';
PRINT '';
PRINT 'Indexes created:';
PRINT '  - Users.Email (Unique)';
PRINT '  - Attendances.UserId_Date (Unique)';
PRINT '  - Attendances.UserId';
PRINT '  - Leaves.UserId';
PRINT '  - Leaves.Status';
PRINT '';
PRINT 'Foreign Keys:';
PRINT '  - Attendances.UserId -> Users.Id (CASCADE DELETE)';
PRINT '  - Leaves.UserId -> Users.Id (CASCADE DELETE)';
PRINT '';
GO

