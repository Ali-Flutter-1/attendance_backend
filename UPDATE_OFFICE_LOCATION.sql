-- SQL Script to Update Office Location
-- Run this in SQL Server Management Studio

USE ATTENDANCE;  -- Replace with your database name if different
GO

-- Update the active office location with new coordinates
UPDATE OfficeLocations
SET 
    Latitude = 31.413239,
    Longitude = 73.0988347,
    AllowedRadiusInMeters = 50.0,
    Name = 'Main Office'
WHERE IsActive = 1;
GO

-- If no active location exists, create one
IF NOT EXISTS (SELECT 1 FROM OfficeLocations WHERE IsActive = 1)
BEGIN
    INSERT INTO OfficeLocations (Name, Latitude, Longitude, AllowedRadiusInMeters, IsActive, CreatedAt)
    VALUES ('Main Office', 31.413239, 73.0988347, 50.0, 1, GETUTCDATE());
END
GO

-- Verify the update
SELECT 
    Id,
    Name,
    Latitude,
    Longitude,
    AllowedRadiusInMeters,
    IsActive
FROM OfficeLocations
WHERE IsActive = 1;
GO

PRINT 'Office location updated successfully!';
PRINT 'New coordinates: 31.413239, 73.0988347';
PRINT 'Radius: 50 meters';
GO

