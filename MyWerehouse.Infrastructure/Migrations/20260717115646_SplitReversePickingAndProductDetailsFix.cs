using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyWerehouse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SplitReversePickingAndProductDetailsFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pallets_Receipts_ReceiptId",
                table: "Pallets");

            migrationBuilder.DropIndex(
                name: "IX_ReversePickings_PickingTaskId",
                table: "ReversePickings");

            migrationBuilder.RenameColumn(
                name: "Quantity",
                table: "HistoryPalletDetails",
                newName: "QuantityChange");

            migrationBuilder.AlterColumn<string>(
                name: "PalletNumber",
                table: "Pallets",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_ReversePickings_PickingTaskId",
                table: "ReversePickings",
                column: "PickingTaskId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Receipts_ReceiptNumber",
                table: "Receipts",
                column: "ReceiptNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pallets_PalletNumber",
                table: "Pallets",
                column: "PalletNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Issues_IssueNumber",
                table: "Issues",
                column: "IssueNumber",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Pallets_Receipts_ReceiptId",
                table: "Pallets",
                column: "ReceiptId",
                principalTable: "Receipts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pallets_Receipts_ReceiptId",
                table: "Pallets");

            migrationBuilder.DropIndex(
                name: "IX_ReversePickings_PickingTaskId",
                table: "ReversePickings");

            migrationBuilder.DropIndex(
                name: "IX_Receipts_ReceiptNumber",
                table: "Receipts");

            migrationBuilder.DropIndex(
                name: "IX_Pallets_PalletNumber",
                table: "Pallets");

            migrationBuilder.DropIndex(
                name: "IX_Issues_IssueNumber",
                table: "Issues");

            migrationBuilder.RenameColumn(
                name: "QuantityChange",
                table: "HistoryPalletDetails",
                newName: "Quantity");

            migrationBuilder.AlterColumn<string>(
                name: "PalletNumber",
                table: "Pallets",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateIndex(
                name: "IX_ReversePickings_PickingTaskId",
                table: "ReversePickings",
                column: "PickingTaskId");

            migrationBuilder.AddForeignKey(
                name: "FK_Pallets_Receipts_ReceiptId",
                table: "Pallets",
                column: "ReceiptId",
                principalTable: "Receipts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
