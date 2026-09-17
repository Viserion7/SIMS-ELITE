using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sims_identidy.Migrations
{
    /// <inheritdoc />
    public partial class adminhinzurüfgen : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_Admin",
                table: "User",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_Admin",
                table: "User");
        }
    }
}
