using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizApi.Migrations
{
    /// <inheritdoc />
    public partial class AddOwnerFKToQuiz : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Quiz_OwnerId",
                table: "Quiz",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Quiz_AspNetUsers_OwnerId",
                table: "Quiz",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Quiz_AspNetUsers_OwnerId",
                table: "Quiz");

            migrationBuilder.DropIndex(
                name: "IX_Quiz_OwnerId",
                table: "Quiz");
        }
    }
}
