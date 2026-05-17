using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyWerehouse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TestMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IssueItems_Issues_IssueId",
                table: "IssueItems");

            migrationBuilder.DropTable(
                name: "PalletMovementDetails");

            migrationBuilder.DropTable(
                name: "PalletMovements");

            migrationBuilder.AlterColumn<string>(
                name: "PalletNumber",
                table: "HistoryPickings",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<Guid>(
                name: "PalletId",
                table: "HistoryPickings",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "PickingPalletId",
                table: "HistoryPickings",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PickingPalletNumber",
                table: "HistoryPickings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "HistoryPallet",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PalletId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PalletNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SourceLocationId = table.Column<int>(type: "int", nullable: true),
                    SourceLocationSnapShot = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DestinationLocationId = table.Column<int>(type: "int", nullable: true),
                    DestinationLocationSnapShot = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PerformedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MovementDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PalletStatus = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoryPallet", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistoryPallet_Pallets_PalletId",
                        column: x => x.PalletId,
                        principalTable: "Pallets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HistoryPalletDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HistoryPalletId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoryPalletDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistoryPalletDetails_HistoryPallet_HistoryPalletId",
                        column: x => x.HistoryPalletId,
                        principalTable: "HistoryPallet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HistoryPallet_PalletId",
                table: "HistoryPallet",
                column: "PalletId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoryPalletDetails_HistoryPalletId",
                table: "HistoryPalletDetails",
                column: "HistoryPalletId");

            migrationBuilder.AddForeignKey(
                name: "FK_IssueItems_Issues_IssueId",
                table: "IssueItems",
                column: "IssueId",
                principalTable: "Issues",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IssueItems_Issues_IssueId",
                table: "IssueItems");

            migrationBuilder.DropTable(
                name: "HistoryPalletDetails");

            migrationBuilder.DropTable(
                name: "HistoryPallet");

            migrationBuilder.DropColumn(
                name: "PickingPalletId",
                table: "HistoryPickings");

            migrationBuilder.DropColumn(
                name: "PickingPalletNumber",
                table: "HistoryPickings");

            migrationBuilder.AlterColumn<string>(
                name: "PalletNumber",
                table: "HistoryPickings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "PalletId",
                table: "HistoryPickings",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "PalletMovements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DestinationLocationId = table.Column<int>(type: "int", nullable: true),
                    DestinationLocationSnapShot = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MovementDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PalletId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PalletNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PalletStatus = table.Column<int>(type: "int", nullable: false),
                    PerformedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SourceLocationId = table.Column<int>(type: "int", nullable: true),
                    SourceLocationSnapShot = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PalletMovements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PalletMovements_Pallets_PalletId",
                        column: x => x.PalletId,
                        principalTable: "Pallets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PalletMovementDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PalletMovementId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PalletMovementDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PalletMovementDetails_PalletMovements_PalletMovementId",
                        column: x => x.PalletMovementId,
                        principalTable: "PalletMovements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PalletMovementDetails_PalletMovementId",
                table: "PalletMovementDetails",
                column: "PalletMovementId");

            migrationBuilder.CreateIndex(
                name: "IX_PalletMovements_PalletId",
                table: "PalletMovements",
                column: "PalletId");

            migrationBuilder.AddForeignKey(
                name: "FK_IssueItems_Issues_IssueId",
                table: "IssueItems",
                column: "IssueId",
                principalTable: "Issues",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
