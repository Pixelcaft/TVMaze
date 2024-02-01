using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TVMaze.Migrations
{
    /// <inheritdoc />
    public partial class updateShowID : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ShowId",
                table: "Shows",
                newName: "ShowID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ShowID",
                table: "Shows",
                newName: "ShowId");
        }
    }
}
