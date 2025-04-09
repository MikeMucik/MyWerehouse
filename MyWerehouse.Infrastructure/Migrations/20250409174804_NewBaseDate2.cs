using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyWerehouse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NewBaseDate2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductInLocations_Locations_LocationId",
                table: "ProductInLocations");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductInLocations_Pallets_PalletId",
                table: "ProductInLocations");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductInLocations_Products_ProductId",
                table: "ProductInLocations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductInLocations",
                table: "ProductInLocations");

            migrationBuilder.RenameTable(
                name: "ProductInLocations",
                newName: "ProductOnPallet");

            migrationBuilder.RenameIndex(
                name: "IX_ProductInLocations_ProductId",
                table: "ProductOnPallet",
                newName: "IX_ProductOnPallet_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductInLocations_PalletId",
                table: "ProductOnPallet",
                newName: "IX_ProductOnPallet_PalletId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductInLocations_LocationId",
                table: "ProductOnPallet",
                newName: "IX_ProductOnPallet_LocationId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductOnPallet",
                table: "ProductOnPallet",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductOnPallet_Locations_LocationId",
                table: "ProductOnPallet",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductOnPallet_Pallets_PalletId",
                table: "ProductOnPallet",
                column: "PalletId",
                principalTable: "Pallets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductOnPallet_Products_ProductId",
                table: "ProductOnPallet",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductOnPallet_Locations_LocationId",
                table: "ProductOnPallet");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductOnPallet_Pallets_PalletId",
                table: "ProductOnPallet");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductOnPallet_Products_ProductId",
                table: "ProductOnPallet");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductOnPallet",
                table: "ProductOnPallet");

            migrationBuilder.RenameTable(
                name: "ProductOnPallet",
                newName: "ProductInLocations");

            migrationBuilder.RenameIndex(
                name: "IX_ProductOnPallet_ProductId",
                table: "ProductInLocations",
                newName: "IX_ProductInLocations_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductOnPallet_PalletId",
                table: "ProductInLocations",
                newName: "IX_ProductInLocations_PalletId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductOnPallet_LocationId",
                table: "ProductInLocations",
                newName: "IX_ProductInLocations_LocationId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductInLocations",
                table: "ProductInLocations",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductInLocations_Locations_LocationId",
                table: "ProductInLocations",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductInLocations_Pallets_PalletId",
                table: "ProductInLocations",
                column: "PalletId",
                principalTable: "Pallets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductInLocations_Products_ProductId",
                table: "ProductInLocations",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
