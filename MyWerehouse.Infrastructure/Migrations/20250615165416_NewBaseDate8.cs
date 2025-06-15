using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyWerehouse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NewBaseDate8 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CartonsPerPallet",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SendedBy",
                table: "Issues",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Issues",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "AdditionalEmail",
                table: "Addresses",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CartonsPerPallet",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SendedBy",
                table: "Issues");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Issues");

            migrationBuilder.DropColumn(
                name: "AdditionalEmail",
                table: "Addresses");
        }
    }
}
