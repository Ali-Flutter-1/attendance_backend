using Microsoft.EntityFrameworkCore;
using attendance.Models;

namespace attendance.Data
{
    /// <summary>
    /// DbContext for Attendance Management System
    /// This class manages all database operations
    /// </summary>
    public class AttendanceDbContext : DbContext
    {
        public AttendanceDbContext(DbContextOptions<AttendanceDbContext> options) : base(options)
        {
        }

        // DbSets - these represent tables in the database
        public DbSet<User> Users { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Leave> Leaves { get; set; }
        public DbSet<OfficeLocation> OfficeLocations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            });

            // Configure Attendance entity
            modelBuilder.Entity<Attendance>(entity =>
            {
                // Ensure one attendance record per user per day
                entity.HasIndex(e => new { e.UserId, e.Date }).IsUnique();
                
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                
                entity.HasOne(e => e.User)
                      .WithMany(u => u.Attendances)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Leave entity
            modelBuilder.Entity<Leave>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                
                entity.HasOne(e => e.User)
                      .WithMany(u => u.Leaves)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure OfficeLocation entity
            modelBuilder.Entity<OfficeLocation>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            });
        }
    }
}

