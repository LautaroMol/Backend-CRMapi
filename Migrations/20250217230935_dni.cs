using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRMapi.Migrations
{
    /// <inheritdoc />
    public partial class dni : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Clients_ClientId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_ClientId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "Orders");

            migrationBuilder.AlterColumn<string>(
                name: "ClientDni",
                table: "Orders",
                type: "nvarchar(8)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Clients_Dni",
                table: "Clients",
                column: "Dni");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ClientDni",
                table: "Orders",
                column: "ClientDni");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Clients_ClientDni",
                table: "Orders",
                column: "ClientDni",
                principalTable: "Clients",
                principalColumn: "Dni",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Clients_ClientDni",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_ClientDni",
                table: "Orders");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Clients_Dni",
                table: "Clients");

            migrationBuilder.AlterColumn<string>(
                name: "ClientDni",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(8)");

            migrationBuilder.AddColumn<int>(
                name: "ClientId",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ClientId",
                table: "Orders",
                column: "ClientId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Clients_ClientId",
                table: "Orders",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
