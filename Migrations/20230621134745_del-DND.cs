using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScheduleBot.Migrations
{
    /// <inheritdoc />
    public partial class delDND : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DNDStart",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "DNDStop",
                table: "Notifications");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeOnly>(
                name: "DNDStart",
                table: "Notifications",
                type: "time without time zone",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "DNDStop",
                table: "Notifications",
                type: "time without time zone",
                nullable: true);
        }
    }
}
