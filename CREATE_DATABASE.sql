-- Create the database if it doesn't exist
CREATE DATABASE IF NOT EXISTS ATTENDANCE CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

-- Grant all privileges to the user
GRANT ALL PRIVILEGES ON ATTENDANCE.* TO 'alihassan'@'%';
FLUSH PRIVILEGES;

