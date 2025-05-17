using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyWerehouse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NewBaseDate7 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Adresses_Clients_ClientId",
                table: "Adresses");

            migrationBuilder.DropForeignKey(
                name: "FK_Pallets_Receipts_ReceiptId",
                table: "Pallets");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Adresses",
                table: "Adresses");

            migrationBuilder.RenameTable(
                name: "Adresses",
                newName: "Addresses");

            migrationBuilder.RenameIndex(
                name: "IX_Adresses_ClientId",
                table: "Addresses",
                newName: "IX_Addresses_ClientId");

            migrationBuilder.AlterColumn<int>(
                name: "ReceiptId",
                table: "Pallets",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Addresses",
                table: "Addresses",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Addresses_Clients_ClientId",
                table: "Addresses",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Pallets_Receipts_ReceiptId",
                table: "Pallets",
                column: "ReceiptId",
                principalTable: "Receipts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Addresses_Clients_ClientId",
                table: "Addresses");

            migrationBuilder.DropForeignKey(
                name: "FK_Pallets_Receipts_ReceiptId",
                table: "Pallets");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Addresses",
                table: "Addresses");

            migrationBuilder.RenameTable(
                name: "Addresses",
                newName: "Adresses");

            migrationBuilder.RenameIndex(
                name: "IX_Addresses_ClientId",
                table: "Adresses",
                newName: "IX_Adresses_ClientId");

            migrationBuilder.AlterColumn<int>(
                name: "ReceiptId",
                table: "Pallets",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Adresses",
                table: "Adresses",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Adresses_Clients_ClientId",
                table: "Adresses",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Pallets_Receipts_ReceiptId",
                table: "Pallets",
                column: "ReceiptId",
                principalTable: "Receipts",
                principalColumn: "Id");
        }
    }
}
