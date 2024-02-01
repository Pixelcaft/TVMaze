using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TVMaze.Migrations
{
    /// <inheritdoc />
    public partial class Removefk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Actors_Shows_ShowID",
                table: "Actors");

            migrationBuilder.DropIndex(
                name: "IX_Actors_ShowID",
                table: "Actors");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Actors_ShowID",
                table: "Actors",
                column: "ShowID");

            migrationBuilder.AddForeignKey(
                name: "FK_Actors_Shows_ShowID",
                table: "Actors",
                column: "ShowID",
                principalTable: "Shows",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
