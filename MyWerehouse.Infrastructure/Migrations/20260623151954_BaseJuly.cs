using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyWerehouse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BaseJuly : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateOnly>(
                name: "IssueDateTimeSend",
                table: "Issues",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.CreateIndex(
                name: "IX_PickingTasks_ProductId",
                table: "PickingTasks",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_PickingTasks_Products_ProductId",
                table: "PickingTasks",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PickingTasks_Products_ProductId",
                table: "PickingTasks");

            migrationBuilder.DropIndex(
                name: "IX_PickingTasks_ProductId",
                table: "PickingTasks");

            migrationBuilder.AlterColumn<DateTime>(
                name: "IssueDateTimeSend",
                table: "Issues",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");
        }
    }
}
