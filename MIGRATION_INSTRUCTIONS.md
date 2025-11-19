# Database Migration Instructions

## Quick Fix: Add PasswordHash Column

The `PasswordHash` column is missing from your database. You have two options:

### Option 1: Run SQL Script Directly (Fastest - No need to stop API)

1. Open **SQL Server Management Studio (SSMS)** or your SQL client
2. Connect to your database server
3. Open the file `ADD_PASSWORD_COLUMN.sql`
4. Make sure the database name matches (currently set to `ATTENDANCE`)
5. Execute the script

This will add the `PasswordHash` column immediately.

### Option 2: Use Entity Framework Migration (Recommended for production)

1. **Stop the running API** (Press Ctrl+C in the terminal where it's running)

2. **Create the migration:**
   ```bash
   cd attendance
   dotnet ef migrations add AddPasswordHashToUser
   ```

3. **Apply the migration:**
   ```bash
   dotnet ef database update
   ```

4. **Restart the API:**
   ```bash
   dotnet run
   ```

## Verify the Column Was Added

Run this SQL query to verify:

```sql
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'PasswordHash';
```

You should see:
- **COLUMN_NAME**: PasswordHash
- **DATA_TYPE**: nvarchar
- **IS_NULLABLE**: YES (nullable for existing users)
- **CHARACTER_MAXIMUM_LENGTH**: 256

## After Adding the Column

Once the column is added, the signup and login APIs will work correctly. Existing users will have `NULL` in the PasswordHash column, and they can set their password via signup.

## Troubleshooting

### Issue: "Cannot alter table because it is being used"

**Solution:**
- Stop the API first
- Run the SQL script
- Restart the API

### Issue: "Database name not found"

**Solution:**
- Check your database name in `appsettings.json`
- Update the `USE` statement in the SQL script to match your database name

### Issue: "Column already exists"

**Solution:**
- The column is already added, you can ignore this error
- Try the signup/login API again

