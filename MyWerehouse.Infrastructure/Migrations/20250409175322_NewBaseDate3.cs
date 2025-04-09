using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyWerehouse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NewBaseDate3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductOnPallet_Locations_LocationId",
                table: "ProductOnPallet");

            migrationBuilder.DropIndex(
                name: "IX_ProductOnPallet_LocationId",
                table: "ProductOnPallet");

            migrationBuilder.DropColumn(
                name: "LocationId",
                table: "ProductOnPallet");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LocationId",
                table: "ProductOnPallet",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductOnPallet_LocationId",
                table: "ProductOnPallet",
                column: "LocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductOnPallet_Locations_LocationId",
                table: "ProductOnPallet",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "Id");
        }
    }
}
