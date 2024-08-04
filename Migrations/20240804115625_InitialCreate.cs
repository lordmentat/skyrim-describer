using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace skyrim_describer.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Locations",
                columns: table => new
                {
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "Npcs",
                columns: table => new
                {
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    LocationName = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Npcs", x => x.Name);
                    table.ForeignKey(
                        name: "FK_Npcs_Locations_LocationName",
                        column: x => x.LocationName,
                        principalTable: "Locations",
                        principalColumn: "Name");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Npcs_LocationName",
                table: "Npcs",
                column: "LocationName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Npcs");

            migrationBuilder.DropTable(
                name: "Locations");
        }
    }
}
