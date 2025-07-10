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
                name: "FK_Inventories_Products_ProductId1",
                table: "Inventories");

            migrationBuilder.DropForeignKey(
                name: "FK_PalletMovements_Locations_LocationId",
                table: "PalletMovements");

            migrationBuilder.DropIndex(
                name: "IX_PalletMovements_LocationId",
                table: "PalletMovements");

            migrationBuilder.DropIndex(
                name: "IX_Inventories_ProductId1",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "LocationId",
                table: "PalletMovements");

            migrationBuilder.DropColumn(
                name: "ProductId1",
                table: "Inventories");

            migrationBuilder.AddColumn<int>(
                name: "DestinationLocationId",
                table: "PalletMovements",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SourceLocationId",
                table: "PalletMovements",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PalletMovements_DestinationLocationId",
                table: "PalletMovements",
                column: "DestinationLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_PalletMovements_SourceLocationId",
                table: "PalletMovements",
                column: "SourceLocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Inventories_Products_ProductId",
                table: "Inventories",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PalletMovements_Locations_DestinationLocationId",
                table: "PalletMovements",
                column: "DestinationLocationId",
                principalTable: "Locations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PalletMovements_Locations_SourceLocationId",
                table: "PalletMovements",
                column: "SourceLocationId",
                principalTable: "Locations",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Inventories_Products_ProductId",
                table: "Inventories");

            migrationBuilder.DropForeignKey(
                name: "FK_PalletMovements_Locations_DestinationLocationId",
                table: "PalletMovements");

            migrationBuilder.DropForeignKey(
                name: "FK_PalletMovements_Locations_SourceLocationId",
                table: "PalletMovements");

            migrationBuilder.DropIndex(
                name: "IX_PalletMovements_DestinationLocationId",
                table: "PalletMovements");

            migrationBuilder.DropIndex(
                name: "IX_PalletMovements_SourceLocationId",
                table: "PalletMovements");

            migrationBuilder.DropColumn(
                name: "DestinationLocationId",
                table: "PalletMovements");

            migrationBuilder.DropColumn(
                name: "SourceLocationId",
                table: "PalletMovements");

            migrationBuilder.AddColumn<int>(
                name: "LocationId",
                table: "PalletMovements",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ProductId1",
                table: "Inventories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_PalletMovements_LocationId",
                table: "PalletMovements",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_ProductId1",
                table: "Inventories",
                column: "ProductId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Inventories_Products_ProductId1",
                table: "Inventories",
                column: "ProductId1",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PalletMovements_Locations_LocationId",
                table: "PalletMovements",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
