using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyWerehouse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitBase2026 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Allocations_VirtualPallets_VirtualPalletId1",
                table: "Allocations");

            migrationBuilder.DropForeignKey(
                name: "FK_HistoryPickings_Allocations_AllocationId",
                table: "HistoryPickings");

            migrationBuilder.DropForeignKey(
                name: "FK_HistoryPickings_Issues_IssueId",
                table: "HistoryPickings");

            migrationBuilder.DropForeignKey(
                name: "FK_HistoryPickings_Issues_IssueId1",
                table: "HistoryPickings");

            migrationBuilder.DropForeignKey(
                name: "FK_HistoryPickings_VirtualPallets_VirtualPalletId",
                table: "HistoryPickings");

            migrationBuilder.DropForeignKey(
                name: "FK_HistoryPickings_VirtualPallets_VirtualPalletId1",
                table: "HistoryPickings");

            migrationBuilder.DropForeignKey(
                name: "FK_PalletMovementDetails_Products_ProductId",
                table: "PalletMovementDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_PalletMovements_Locations_DestinationLocationId",
                table: "PalletMovements");

            migrationBuilder.DropForeignKey(
                name: "FK_PalletMovements_Locations_SourceLocationId",
                table: "PalletMovements");

            migrationBuilder.DropForeignKey(
                name: "FK_Pallets_Issues_IssueId",
                table: "Pallets");

            migrationBuilder.DropIndex(
                name: "IX_PalletMovements_DestinationLocationId",
                table: "PalletMovements");

            migrationBuilder.DropIndex(
                name: "IX_PalletMovements_SourceLocationId",
                table: "PalletMovements");

            migrationBuilder.DropIndex(
                name: "IX_PalletMovementDetails_ProductId",
                table: "PalletMovementDetails");

            migrationBuilder.DropIndex(
                name: "IX_HistoryPickings_IssueId1",
                table: "HistoryPickings");

            migrationBuilder.DropIndex(
                name: "IX_HistoryPickings_VirtualPalletId1",
                table: "HistoryPickings");

            migrationBuilder.DropIndex(
                name: "IX_Allocations_VirtualPalletId1",
                table: "Allocations");

            migrationBuilder.DropColumn(
                name: "SendedBy",
                table: "Issues");

            migrationBuilder.DropColumn(
                name: "IssueId1",
                table: "HistoryPickings");

            migrationBuilder.DropColumn(
                name: "VirtualPalletId1",
                table: "HistoryPickings");

            migrationBuilder.DropColumn(
                name: "VirtualPalletId1",
                table: "Allocations");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "HistoryReceipts",
                newName: "StatusAfter");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "HistoryIssues",
                newName: "StatusAfter");

            migrationBuilder.AlterColumn<string>(
                name: "PerformedBy",
                table: "Receipts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RampNumber",
                table: "Receipts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Pallets",
                type: "rowversion",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PerformedBy",
                table: "PalletMovements",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DestinationLocationSnapShot",
                table: "PalletMovements",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceLocationSnapShot",
                table: "PalletMovements",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PerformedBy",
                table: "Issues",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "IssueDateTimeSend",
                table: "Issues",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PerformedBy",
                table: "HistoryReceipts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ClientId",
                table: "HistoryReceipts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "LocationSnapShot",
                table: "HistoryReceiptDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "VirtualPalletId",
                table: "HistoryPickings",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "PerformedBy",
                table: "HistoryPickings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "AllocationId",
                table: "HistoryPickings",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "PalletId",
                table: "HistoryPickings",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "PerformedBy",
                table: "HistoryIssues",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ClientId",
                table: "HistoryIssues",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "LocationSnapShot",
                table: "HistoryIssueDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "HistoryIssueItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    BestBefore = table.Column<DateOnly>(type: "date", nullable: false),
                    HistoryIssueId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoryIssueItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistoryIssueItems_HistoryIssues_HistoryIssueId",
                        column: x => x.HistoryIssueId,
                        principalTable: "HistoryIssues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HistoryReversePickings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReversePickingId = table.Column<int>(type: "int", nullable: false),
                    PalletSourceId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PalletDestinationId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IssueId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    StatusBefore = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StatusAfter = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PerformedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoryReversePickings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IssueItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IssueId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    BestBefore = table.Column<DateOnly>(type: "date", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IssueItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IssueItems_Issues_IssueId",
                        column: x => x.IssueId,
                        principalTable: "Issues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IssueItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReversePickings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PickingPalletId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SourcePalletId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DestinationPalletId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    BestBefore = table.Column<DateOnly>(type: "date", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    AllocationId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReversePickings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReversePickings_Allocations_AllocationId",
                        column: x => x.AllocationId,
                        principalTable: "Allocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HistoryPickings_DateTime",
                table: "HistoryPickings",
                column: "DateTime");

            migrationBuilder.CreateIndex(
                name: "IX_HistoryPickings_PalletId",
                table: "HistoryPickings",
                column: "PalletId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoryIssueItems_HistoryIssueId",
                table: "HistoryIssueItems",
                column: "HistoryIssueId");

            migrationBuilder.CreateIndex(
                name: "IX_IssueItems_IssueId",
                table: "IssueItems",
                column: "IssueId");

            migrationBuilder.CreateIndex(
                name: "IX_IssueItems_ProductId",
                table: "IssueItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ReversePickings_AllocationId",
                table: "ReversePickings",
                column: "AllocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_HistoryPickings_Issues_IssueId",
                table: "HistoryPickings",
                column: "IssueId",
                principalTable: "Issues",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_HistoryPickings_VirtualPallets_VirtualPalletId",
                table: "HistoryPickings",
                column: "VirtualPalletId",
                principalTable: "VirtualPallets",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Pallets_Issues_IssueId",
                table: "Pallets",
                column: "IssueId",
                principalTable: "Issues",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HistoryPickings_Issues_IssueId",
                table: "HistoryPickings");

            migrationBuilder.DropForeignKey(
                name: "FK_HistoryPickings_VirtualPallets_VirtualPalletId",
                table: "HistoryPickings");

            migrationBuilder.DropForeignKey(
                name: "FK_Pallets_Issues_IssueId",
                table: "Pallets");

            migrationBuilder.DropTable(
                name: "HistoryIssueItems");

            migrationBuilder.DropTable(
                name: "HistoryReversePickings");

            migrationBuilder.DropTable(
                name: "IssueItems");

            migrationBuilder.DropTable(
                name: "ReversePickings");

            migrationBuilder.DropIndex(
                name: "IX_HistoryPickings_DateTime",
                table: "HistoryPickings");

            migrationBuilder.DropIndex(
                name: "IX_HistoryPickings_PalletId",
                table: "HistoryPickings");

            migrationBuilder.DropColumn(
                name: "RampNumber",
                table: "Receipts");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Pallets");

            migrationBuilder.DropColumn(
                name: "DestinationLocationSnapShot",
                table: "PalletMovements");

            migrationBuilder.DropColumn(
                name: "SourceLocationSnapShot",
                table: "PalletMovements");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "HistoryReceipts");

            migrationBuilder.DropColumn(
                name: "LocationSnapShot",
                table: "HistoryReceiptDetails");

            migrationBuilder.DropColumn(
                name: "PalletId",
                table: "HistoryPickings");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "HistoryIssues");

            migrationBuilder.DropColumn(
                name: "LocationSnapShot",
                table: "HistoryIssueDetails");

            migrationBuilder.RenameColumn(
                name: "StatusAfter",
                table: "HistoryReceipts",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "StatusAfter",
                table: "HistoryIssues",
                newName: "Status");

            migrationBuilder.AlterColumn<string>(
                name: "PerformedBy",
                table: "Receipts",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "PerformedBy",
                table: "PalletMovements",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "PerformedBy",
                table: "Issues",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "IssueDateTimeSend",
                table: "Issues",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<string>(
                name: "SendedBy",
                table: "Issues",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PerformedBy",
                table: "HistoryReceipts",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "VirtualPalletId",
                table: "HistoryPickings",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PerformedBy",
                table: "HistoryPickings",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "AllocationId",
                table: "HistoryPickings",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IssueId1",
                table: "HistoryPickings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VirtualPalletId1",
                table: "HistoryPickings",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PerformedBy",
                table: "HistoryIssues",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "VirtualPalletId1",
                table: "Allocations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_PalletMovements_DestinationLocationId",
                table: "PalletMovements",
                column: "DestinationLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_PalletMovements_SourceLocationId",
                table: "PalletMovements",
                column: "SourceLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_PalletMovementDetails_ProductId",
                table: "PalletMovementDetails",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoryPickings_IssueId1",
                table: "HistoryPickings",
                column: "IssueId1");

            migrationBuilder.CreateIndex(
                name: "IX_HistoryPickings_VirtualPalletId1",
                table: "HistoryPickings",
                column: "VirtualPalletId1");

            migrationBuilder.CreateIndex(
                name: "IX_Allocations_VirtualPalletId1",
                table: "Allocations",
                column: "VirtualPalletId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Allocations_VirtualPallets_VirtualPalletId1",
                table: "Allocations",
                column: "VirtualPalletId1",
                principalTable: "VirtualPallets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_HistoryPickings_Allocations_AllocationId",
                table: "HistoryPickings",
                column: "AllocationId",
                principalTable: "Allocations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_HistoryPickings_Issues_IssueId",
                table: "HistoryPickings",
                column: "IssueId",
                principalTable: "Issues",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_HistoryPickings_Issues_IssueId1",
                table: "HistoryPickings",
                column: "IssueId1",
                principalTable: "Issues",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_HistoryPickings_VirtualPallets_VirtualPalletId",
                table: "HistoryPickings",
                column: "VirtualPalletId",
                principalTable: "VirtualPallets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_HistoryPickings_VirtualPallets_VirtualPalletId1",
                table: "HistoryPickings",
                column: "VirtualPalletId1",
                principalTable: "VirtualPallets",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PalletMovementDetails_Products_ProductId",
                table: "PalletMovementDetails",
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

            migrationBuilder.AddForeignKey(
                name: "FK_Pallets_Issues_IssueId",
                table: "Pallets",
                column: "IssueId",
                principalTable: "Issues",
                principalColumn: "Id");
        }
    }
}
