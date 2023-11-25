using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payment.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class stripe_integration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StripePriceId",
                table: "Subcriptions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StripeProductId",
                table: "Subcriptions",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StripePriceId",
                table: "Subcriptions");

            migrationBuilder.DropColumn(
                name: "StripeProductId",
                table: "Subcriptions");
        }
    }
}
