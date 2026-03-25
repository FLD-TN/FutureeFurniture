using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MTKPM_FE.Migrations
{
    public partial class SeedAdmin : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "tbl_admin",
                columns: new[] { "admin_id", "admin_email", "admin_image", "admin_name", "admin_password" },
                values: new object[] { 1, "admin@gmail.com", "default-admin.png", "Administrator", "admin123" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "tbl_admin",
                keyColumn: "admin_id",
                keyValue: 1);
        }
    }
}
