using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EbookApp.Migrations
{
    /// <inheritdoc />
    public partial class addfieldprice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "UnitPrice",
                table: "CartDetail",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UnitPrice",
                table: "CartDetail");
        }
    }
}
