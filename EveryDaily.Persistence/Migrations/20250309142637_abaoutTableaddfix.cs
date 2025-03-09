using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EveryDaily.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class abaoutTableaddfix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Faculties_Faculties_FacultyEntityId",
                table: "Faculties");

            migrationBuilder.DropIndex(
                name: "IX_Faculties_FacultyEntityId",
                table: "Faculties");

            migrationBuilder.DropColumn(
                name: "FacultyEntityId",
                table: "Faculties");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "FacultyEntityId",
                table: "Faculties",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Faculties_FacultyEntityId",
                table: "Faculties",
                column: "FacultyEntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Faculties_Faculties_FacultyEntityId",
                table: "Faculties",
                column: "FacultyEntityId",
                principalTable: "Faculties",
                principalColumn: "Id");
        }
    }
}
