using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASAssignment.Migrations
{
    public partial class HashMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PreviousPasswordHash1",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PreviousPasswordHash2",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PreviousPasswordHash1",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PreviousPasswordHash2",
                table: "AspNetUsers");
        }
    }
}
