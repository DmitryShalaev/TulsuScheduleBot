using Microsoft.EntityFrameworkCore.Migrations;

using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ScheduleBot.Migrations {
    /// <inheritdoc />
    public partial class InitialCreate : Migration {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.CreateTable(
                name: "CompletedDisciplines",
                columns: table => new {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Lecturer = table.Column<string>(type: "text", nullable: true),
                    Class = table.Column<byte>(type: "smallint", nullable: false)
                },
                constraints: table => {
                    table.PrimaryKey("PK_CompletedDisciplines", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Disciplines",
                columns: table => new {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Lecturer = table.Column<string>(type: "text", nullable: true),
                    LectureHall = table.Column<string>(type: "text", nullable: false),
                    Subgroup = table.Column<string>(type: "text", nullable: true),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    Class = table.Column<byte>(type: "smallint", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table => {
                    table.PrimaryKey("PK_Disciplines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TelegramUsers",
                columns: table => new {
                    ChatId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: true),
                    Username = table.Column<string>(type: "text", nullable: true),
                    IsAdmin = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table => {
                    table.PrimaryKey("PK_TelegramUsers", x => x.ChatId);
                });

            migrationBuilder.CreateTable(
                name: "Types",
                columns: table => new {
                    Id = table.Column<byte>(type: "smallint", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table => {
                    table.PrimaryKey("PK_Types", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Types",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { (byte)0, "all" },
                    { (byte)1, "lab" },
                    { (byte)2, "practice" },
                    { (byte)3, "lecture" },
                    { (byte)4, "other" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropTable(
                name: "CompletedDisciplines");

            migrationBuilder.DropTable(
                name: "Disciplines");

            migrationBuilder.DropTable(
                name: "TelegramUsers");

            migrationBuilder.DropTable(
                name: "Types");
        }
    }
}
