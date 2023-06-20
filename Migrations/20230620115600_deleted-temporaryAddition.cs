using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ScheduleBot.Migrations
{
    /// <inheritdoc />
    public partial class deletedtemporaryAddition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TemporaryAddition");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "CustomDiscipline",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<TimeOnly>(
                name: "StartTime",
                table: "CustomDiscipline",
                type: "time without time zone",
                nullable: true,
                oldClrType: typeof(TimeOnly),
                oldType: "time without time zone");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "CustomDiscipline",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "LectureHall",
                table: "CustomDiscipline",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<TimeOnly>(
                name: "EndTime",
                table: "CustomDiscipline",
                type: "time without time zone",
                nullable: true,
                oldClrType: typeof(TimeOnly),
                oldType: "time without time zone");

            migrationBuilder.AddColumn<DateTime>(
                name: "AddDate",
                table: "CustomDiscipline",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "Counter",
                table: "CustomDiscipline",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsAdded",
                table: "CustomDiscipline",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddDate",
                table: "CustomDiscipline");

            migrationBuilder.DropColumn(
                name: "Counter",
                table: "CustomDiscipline");

            migrationBuilder.DropColumn(
                name: "IsAdded",
                table: "CustomDiscipline");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "CustomDiscipline",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<TimeOnly>(
                name: "StartTime",
                table: "CustomDiscipline",
                type: "time without time zone",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0),
                oldClrType: typeof(TimeOnly),
                oldType: "time without time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "CustomDiscipline",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LectureHall",
                table: "CustomDiscipline",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<TimeOnly>(
                name: "EndTime",
                table: "CustomDiscipline",
                type: "time without time zone",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0),
                oldClrType: typeof(TimeOnly),
                oldType: "time without time zone",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "TemporaryAddition",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    User = table.Column<long>(type: "bigint", nullable: false),
                    AddDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Counter = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    LectureHall = table.Column<string>(type: "text", nullable: true),
                    Lecturer = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true),
                    StartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    Type = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemporaryAddition", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TemporaryAddition_TelegramUsers_User",
                        column: x => x.User,
                        principalTable: "TelegramUsers",
                        principalColumn: "ChatID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TemporaryAddition_User",
                table: "TemporaryAddition",
                column: "User");
        }
    }
}
