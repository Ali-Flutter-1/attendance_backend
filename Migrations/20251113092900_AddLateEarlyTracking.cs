using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace attendance.Migrations
{
    /// <inheritdoc />
    public partial class AddLateEarlyTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsEarlyCheckOut",
                table: "Attendances",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsLateCheckIn",
                table: "Attendances",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsEarlyCheckOut",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "IsLateCheckIn",
                table: "Attendances");
        }
    }
}
