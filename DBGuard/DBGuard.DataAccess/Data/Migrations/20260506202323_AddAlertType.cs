using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DBGuard.DataAccess.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAlertType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte>(
                name: "Type",
                table: "Alerts",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "Alerts");
        }
    }
}
