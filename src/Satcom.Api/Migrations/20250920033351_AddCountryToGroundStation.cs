using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Satcom.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCountryToGroundStation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "GroundStations",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Country",
                table: "GroundStations");
        }
    }
}
