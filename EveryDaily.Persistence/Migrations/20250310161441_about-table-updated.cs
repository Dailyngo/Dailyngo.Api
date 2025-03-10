using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EveryDaily.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class abouttableupdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "BirthDate",
                table: "Abouts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Gender",
                table: "Abouts",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BirthDate",
                table: "Abouts");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "Abouts");
        }
    }
}
