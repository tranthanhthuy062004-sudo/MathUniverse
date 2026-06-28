using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MathUniverse.Migrations
{
    /// <inheritdoc />
    public partial class AddPdfAndEssaySupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Questions",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PdfUrl",
                table: "Lessons",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EssayAnswers",
                columns: table => new
                {
                    EssayAnswerId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ExerciseResultId = table.Column<int>(type: "INTEGER", nullable: false),
                    QuestionId = table.Column<int>(type: "INTEGER", nullable: false),
                    AnswerText = table.Column<string>(type: "TEXT", nullable: false),
                    Score = table.Column<double>(type: "REAL", nullable: true),
                    Feedback = table.Column<string>(type: "TEXT", nullable: true),
                    SubmittedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GradedDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    GradedByAdminId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EssayAnswers", x => x.EssayAnswerId);
                    table.ForeignKey(
                        name: "FK_EssayAnswers_ExerciseResults_ExerciseResultId",
                        column: x => x.ExerciseResultId,
                        principalTable: "ExerciseResults",
                        principalColumn: "ResultId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EssayAnswers_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "QuestionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EssayAnswers_ExerciseResultId",
                table: "EssayAnswers",
                column: "ExerciseResultId");

            migrationBuilder.CreateIndex(
                name: "IX_EssayAnswers_QuestionId",
                table: "EssayAnswers",
                column: "QuestionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EssayAnswers");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "PdfUrl",
                table: "Lessons");
        }
    }
}
