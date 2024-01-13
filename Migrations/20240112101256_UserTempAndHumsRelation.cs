using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClimateTrackr_Server.Migrations
{
    /// <inheritdoc />
    public partial class UserTempAndHumsRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "TempAndHums",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TempAndHums_UserId",
                table: "TempAndHums",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_TempAndHums_Users_UserId",
                table: "TempAndHums",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TempAndHums_Users_UserId",
                table: "TempAndHums");

            migrationBuilder.DropIndex(
                name: "IX_TempAndHums_UserId",
                table: "TempAndHums");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "TempAndHums");
        }
    }
}
