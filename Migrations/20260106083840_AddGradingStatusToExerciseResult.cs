using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MathUniverse.Migrations
{
    /// <inheritdoc />
    public partial class AddGradingStatusToExerciseResult : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GradingStatus",
                table: "ExerciseResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GradingStatus",
                table: "ExerciseResults");
        }
    }
}
