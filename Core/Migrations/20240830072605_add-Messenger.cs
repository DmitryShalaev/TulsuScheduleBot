using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ScheduleBot.Migrations
{
    /// <inheritdoc />
    public partial class addMessenger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Messenger",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    From = table.Column<long>(type: "bigint", nullable: false),
                    Previous = table.Column<long>(type: "bigint", nullable: true),
                    Following = table.Column<long>(type: "bigint", nullable: true),
                    FeedbackID = table.Column<long>(type: "bigint", nullable: true),
                    Message = table.Column<string>(type: "text", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messenger", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Messenger_Feedbacks_FeedbackID",
                        column: x => x.FeedbackID,
                        principalTable: "Feedbacks",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_Messenger_Messenger_Following",
                        column: x => x.Following,
                        principalTable: "Messenger",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_Messenger_Messenger_Previous",
                        column: x => x.Previous,
                        principalTable: "Messenger",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_Messenger_TelegramUsers_From",
                        column: x => x.From,
                        principalTable: "TelegramUsers",
                        principalColumn: "ChatID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Messenger_FeedbackID",
                table: "Messenger",
                column: "FeedbackID");

            migrationBuilder.CreateIndex(
                name: "IX_Messenger_Following",
                table: "Messenger",
                column: "Following");

            migrationBuilder.CreateIndex(
                name: "IX_Messenger_From",
                table: "Messenger",
                column: "From");

            migrationBuilder.CreateIndex(
                name: "IX_Messenger_Previous",
                table: "Messenger",
                column: "Previous");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Messenger");
        }
    }
}
