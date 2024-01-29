using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASAssignment.Migrations
{
    public partial class SessionIDMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StoredSessionId",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StoredSessionId",
                table: "AspNetUsers");
        }
    }
}
