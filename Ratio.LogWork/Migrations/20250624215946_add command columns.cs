using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ratio.LogWork.Migrations
{
    /// <inheritdoc />
    public partial class addcommandcolumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Command",
                table: "WorkLogs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FullCommand",
                table: "WorkLogs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Command",
                table: "WorkLogs");

            migrationBuilder.DropColumn(
                name: "FullCommand",
                table: "WorkLogs");
        }
    }
}
