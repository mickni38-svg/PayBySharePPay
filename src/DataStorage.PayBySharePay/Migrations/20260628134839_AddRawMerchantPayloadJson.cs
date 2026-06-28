using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataStorage.PayBySharePay.Migrations
{
    /// <inheritdoc />
    public partial class AddRawMerchantPayloadJson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RawMerchantPayloadJson",
                table: "MerchantOrderDrafts",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RawMerchantPayloadJson",
                table: "MerchantOrderDrafts");
        }
    }
}
