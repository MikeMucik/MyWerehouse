using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyWerehouse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Correct1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Issues_Products_ProductId",
                table: "Issues");

            migrationBuilder.DropForeignKey(
                name: "FK_Receipts_Products_ProductId",
                table: "Receipts");

            migrationBuilder.DropIndex(
                name: "IX_Receipts_ProductId",
                table: "Receipts");

            migrationBuilder.DropIndex(
                name: "IX_Issues_ProductId",
                table: "Issues");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "Receipts");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "Issues");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ProductId",
                table: "Receipts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProductId",
                table: "Issues",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Receipts_ProductId",
                table: "Receipts",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Issues_ProductId",
                table: "Issues",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_Issues_Products_ProductId",
                table: "Issues",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Receipts_Products_ProductId",
                table: "Receipts",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id");
        }
    }
}
