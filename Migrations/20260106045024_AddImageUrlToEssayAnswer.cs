using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MathUniverse.Migrations
{
    /// <inheritdoc />
    public partial class AddImageUrlToEssayAnswer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "EssayAnswers",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "EssayAnswers");
        }
    }
}
