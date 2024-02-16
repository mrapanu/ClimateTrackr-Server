using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClimateTrackr_Server.Migrations
{
    /// <inheritdoc />
    public partial class AddRoomIdToReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RoomId",
                table: "Reports",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RoomId",
                table: "Reports");
        }
    }
}
