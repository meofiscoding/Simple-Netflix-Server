using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payment.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class add_UserPayment_table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserPayment_Subcriptions_SubcriptionId",
                table: "UserPayment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserPayment",
                table: "UserPayment");

            migrationBuilder.DropColumn(
                name: "Amount",
                table: "UserPayment");

            migrationBuilder.RenameTable(
                name: "UserPayment",
                newName: "UserPayments");

            migrationBuilder.RenameIndex(
                name: "IX_UserPayment_SubcriptionId",
                table: "UserPayments",
                newName: "IX_UserPayments_SubcriptionId");

            migrationBuilder.AlterColumn<int>(
                name: "Price",
                table: "Subcriptions",
                type: "integer",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AddColumn<bool>(
                name: "IsCancelled",
                table: "UserPayments",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserPayments",
                table: "UserPayments",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserPayments_Subcriptions_SubcriptionId",
                table: "UserPayments",
                column: "SubcriptionId",
                principalTable: "Subcriptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserPayments_Subcriptions_SubcriptionId",
                table: "UserPayments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserPayments",
                table: "UserPayments");

            migrationBuilder.DropColumn(
                name: "IsCancelled",
                table: "UserPayments");

            migrationBuilder.RenameTable(
                name: "UserPayments",
                newName: "UserPayment");

            migrationBuilder.RenameIndex(
                name: "IX_UserPayments_SubcriptionId",
                table: "UserPayment",
                newName: "IX_UserPayment_SubcriptionId");

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "Subcriptions",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<decimal>(
                name: "Amount",
                table: "UserPayment",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserPayment",
                table: "UserPayment",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserPayment_Subcriptions_SubcriptionId",
                table: "UserPayment",
                column: "SubcriptionId",
                principalTable: "Subcriptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
