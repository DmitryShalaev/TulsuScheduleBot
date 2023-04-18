using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScheduleBot.Migrations {
    /// <inheritdoc />
    public partial class AddTemporaryAdditionType : Migration {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.AlterColumn<DateOnly>(
                name: "Date",
                table: "TemporaryAddition",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1),
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "TemporaryAddition",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "TemporaryAddition");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "Date",
                table: "TemporaryAddition",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date");
        }
    }
}
