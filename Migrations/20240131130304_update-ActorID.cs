using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TVMaze.Migrations
{
    /// <inheritdoc />
    public partial class updateActorID : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ActorId",
                table: "Actors",
                newName: "ActorID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ActorID",
                table: "Actors",
                newName: "ActorId");
        }
    }
}
