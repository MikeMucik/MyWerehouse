using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyWerehouse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BestBeforeForHistoryPallet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "QuantityChange",
                table: "HistoryPalletDetails",
                newName: "Quantity");

            migrationBuilder.AddColumn<DateOnly>(
                name: "BestBefore",
                table: "HistoryPalletDetails",
                type: "date",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BestBefore",
                table: "HistoryPalletDetails");

            migrationBuilder.RenameColumn(
                name: "Quantity",
                table: "HistoryPalletDetails",
                newName: "QuantityChange");
        }
    }
}
