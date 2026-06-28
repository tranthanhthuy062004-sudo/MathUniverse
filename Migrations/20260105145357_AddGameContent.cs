using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MathUniverse.Migrations
{
    /// <inheritdoc />
    public partial class AddGameContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GameContents",
                columns: table => new
                {
                    GameContentId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LessonId = table.Column<int>(type: "INTEGER", nullable: false),
                    CardQuestion = table.Column<string>(type: "TEXT", nullable: false),
                    CardAnswer = table.Column<string>(type: "TEXT", nullable: false),
                    OrderIndex = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameContents", x => x.GameContentId);
                    table.ForeignKey(
                        name: "FK_GameContents_Lessons_LessonId",
                        column: x => x.LessonId,
                        principalTable: "Lessons",
                        principalColumn: "LessonId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GameContents_LessonId",
                table: "GameContents",
                column: "LessonId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GameContents");
        }
    }
}
