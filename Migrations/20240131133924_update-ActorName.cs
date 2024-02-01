using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TVMaze.Migrations
{
    /// <inheritdoc />
    public partial class updateActorName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ActorNames",
                table: "Actors",
                newName: "ActorName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ActorName",
                table: "Actors",
                newName: "ActorNames");
        }
    }
}
