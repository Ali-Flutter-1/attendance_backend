-- =============================================
-- Attendance Management System - Simple CREATE TABLE Script
-- Run this script to create all tables
-- =============================================

USE ATTENDANCE;  -- Replace with your database name if different
GO

-- =============================================
-- 1. USERS TABLE
-- =============================================
CREATE TABLE [dbo].[Users] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [FirstName] NVARCHAR(50) NOT NULL,
    [LastName] NVARCHAR(50) NOT NULL,
    [Email] NVARCHAR(100) NOT NULL UNIQUE,
    [PasswordHash] NVARCHAR(256) NULL,
    [Domain] NVARCHAR(100) NULL,
    [Address] NVARCHAR(500) NULL,
    [ProfilePicturePath] NVARCHAR(500) NULL,
    [IsAdmin] BIT NOT NULL DEFAULT 0,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2 NULL
);
GO

-- =============================================
-- 2. OFFICE LOCATIONS TABLE
-- =============================================
CREATE TABLE [dbo].[OfficeLocations] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Name] NVARCHAR(200) NOT NULL DEFAULT 'Main Office',
    [Latitude] FLOAT NOT NULL,
    [Longitude] FLOAT NOT NULL,
    [AllowedRadiusInMeters] FLOAT NOT NULL DEFAULT 50.0,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);
GO

-- =============================================
-- 3. ATTENDANCES TABLE
-- =============================================
CREATE TABLE [dbo].[Attendances] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
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
    
    FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id]) ON DELETE CASCADE,
    UNIQUE ([UserId], [Date])  -- One attendance per user per day
);
GO

-- Create index on UserId for faster lookups
CREATE INDEX [IX_Attendances_UserId] ON [dbo].[Attendances] ([UserId]);
GO

-- =============================================
-- 4. LEAVES TABLE
-- =============================================
CREATE TABLE [dbo].[Leaves] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [UserId] INT NOT NULL,
    [Type] INT NOT NULL,  -- 1=Sick, 2=Casual, 3=Annual, 4=Emergency, 5=Other
    [Reason] NVARCHAR(1000) NOT NULL,
    [StartDate] DATE NOT NULL,
    [EndDate] DATE NOT NULL,
    [Status] INT NOT NULL DEFAULT 1,  -- 1=Pending, 2=Approved, 3=Declined
    [AdminRemarks] NVARCHAR(500) NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2 NULL,
    
    FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id]) ON DELETE CASCADE
);
GO

-- Create indexes for faster lookups
CREATE INDEX [IX_Leaves_UserId] ON [dbo].[Leaves] ([UserId]);
CREATE INDEX [IX_Leaves_Status] ON [dbo].[Leaves] ([Status]);
GO

PRINT 'All tables created successfully!';
PRINT 'Primary Keys: Users.Id, OfficeLocations.Id, Attendances.Id, Leaves.Id';
PRINT 'Foreign Keys: Attendances.UserId -> Users.Id, Leaves.UserId -> Users.Id';
GO

