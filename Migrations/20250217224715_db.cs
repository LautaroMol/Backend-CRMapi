using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRMapi.Migrations
{
    /// <inheritdoc />
    public partial class db : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CustomerDni",
                table: "Orders",
                newName: "ClientDni");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ClientDni",
                table: "Orders",
                newName: "CustomerDni");
        }
    }
}
