# Authentication Migration Guide

## Database Migration Required

After adding password authentication, you need to update the database schema.

## Step 1: Create Migration

```bash
cd attendance
dotnet ef migrations add AddPasswordToUser
```

This will create a migration file that adds the `PasswordHash` column to the `Users` table.

## Step 2: Apply Migration

```bash
dotnet ef database update
```

This will update your database with the new `PasswordHash` column.

## Step 3: Update Existing Users (Optional)

If you have existing users in the database, you'll need to set passwords for them.

### Option 1: Update via SQL

```sql
-- Set a default password for existing users
-- Password: "password123" (hashed)
UPDATE Users
SET PasswordHash = 'EF92B778BAFE771E89245B89ECBC08A44A4E166C06659911881F383D4473E94F'
WHERE PasswordHash IS NULL OR PasswordHash = '';

-- Note: The hash above is for "password123"
-- You should use the PasswordService to generate proper hashes
```

### Option 2: Use the Signup API

Have existing users sign up again, or use the API to update their passwords.

## Step 4: Test

1. Test signup:
   ```bash
   POST /api/auth/signup
   {
     "email": "newuser@example.com",
     "password": "test123",
     "confirmPassword": "test123"
   }
   ```

2. Test login:
   ```bash
   POST /api/auth/login
   {
     "email": "newuser@example.com",
     "password": "test123"
   }
   ```

## Migration File Preview

The migration will add:
- `PasswordHash` column (nvarchar(256), NOT NULL)
- Default value handling for existing records

## Rollback (if needed)

If you need to rollback the migration:

```bash
dotnet ef database update PreviousMigrationName
```

Or manually remove the column:
```sql
ALTER TABLE Users DROP COLUMN PasswordHash;
```

