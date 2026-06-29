using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyWerehouse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AzureInitialFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Locations_Bay_Aisle_Position_Height",
                table: "Locations",
                columns: new[] { "Bay", "Aisle", "Position", "Height" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Locations_Bay_Aisle_Position_Height",
                table: "Locations");
        }
    }
}
