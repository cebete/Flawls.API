using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Flawls.API.Migrations
{
    /// <inheritdoc />
    public partial class AddLowStockThreshold : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LowStockThreshold",
                table: "Products",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LowStockThreshold",
                table: "Products");
        }
    }
}
