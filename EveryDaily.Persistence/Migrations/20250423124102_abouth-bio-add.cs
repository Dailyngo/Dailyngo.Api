using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EveryDaily.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class abouthbioadd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Bio",
                table: "Abouts",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Bio",
                table: "Abouts");
        }
    }
}
