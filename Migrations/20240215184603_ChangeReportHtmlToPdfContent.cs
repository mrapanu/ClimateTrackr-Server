using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClimateTrackr_Server.Migrations
{
    /// <inheritdoc />
    public partial class ChangeReportHtmlToPdfContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "HtmlContent",
                table: "Reports",
                newName: "PdfContent");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PdfContent",
                table: "Reports",
                newName: "HtmlContent");
        }
    }
}
