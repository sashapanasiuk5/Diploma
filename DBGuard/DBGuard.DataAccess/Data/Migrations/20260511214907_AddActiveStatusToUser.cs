using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DBGuard.DataAccess.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddActiveStatusToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ReceiveAlerts",
                table: "Users",
                newName: "IsActive");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "Users",
                newName: "ReceiveAlerts");
        }
    }
}
