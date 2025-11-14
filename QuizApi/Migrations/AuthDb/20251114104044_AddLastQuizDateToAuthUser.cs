using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizApi.Migrations.AuthDb
{
    /// <inheritdoc />
    public partial class AddLastQuizDateToAuthUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastQuizDate",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Streak",
                table: "AspNetUsers",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastQuizDate",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Streak",
                table: "AspNetUsers");
        }
    }
}
