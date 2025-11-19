-- SQL Script to Add PasswordHash Column to Users Table
-- Run this script in SQL Server Management Studio or your SQL client

USE ATTENDANCE;  -- Replace with your database name if different
GO

-- Add PasswordHash column (nullable for existing users)
ALTER TABLE Users
ADD PasswordHash NVARCHAR(256) NULL;
GO

-- Optional: Add a comment/description
EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Stores SHA256 hashed password for user authentication', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'Users', 
    @level2type = N'COLUMN', @level2name = N'PasswordHash';
GO

-- Verify the column was added
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'PasswordHash';
GO

PRINT 'PasswordHash column added successfully!';
GO

