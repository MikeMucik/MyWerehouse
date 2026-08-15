using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyWerehouse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNumberCounter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NumberCounters",
                columns: table => new
                {
                    Name = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    NextNumber = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NumberCounters", x => x.Name);
                });

            migrationBuilder.InsertData(
                table: "NumberCounters",
                columns: new[] { "Name", "NextNumber" },
                values: new object[] { "Pallet", 1 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NumberCounters");
        }
    }
}
