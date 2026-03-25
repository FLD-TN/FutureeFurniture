using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MTKPM_FE.Migrations
{
    public partial class AddFeedbackColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "tbl_feedback",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "tbl_feedback",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "CustomerId",
                table: "tbl_feedback",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Rating",
                table: "tbl_feedback",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "tbl_customer",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_tbl_feedback_CustomerId",
                table: "tbl_feedback",
                column: "CustomerId");

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_feedback_tbl_customer_CustomerId",
                table: "tbl_feedback",
                column: "CustomerId",
                principalTable: "tbl_customer",
                principalColumn: "customer_id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tbl_feedback_tbl_customer_CustomerId",
                table: "tbl_feedback");

            migrationBuilder.DropIndex(
                name: "IX_tbl_feedback_CustomerId",
                table: "tbl_feedback");

            migrationBuilder.DropColumn(
                name: "Content",
                table: "tbl_feedback");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "tbl_feedback");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "tbl_feedback");

            migrationBuilder.DropColumn(
                name: "Rating",
                table: "tbl_feedback");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "tbl_customer");
        }
    }
}
