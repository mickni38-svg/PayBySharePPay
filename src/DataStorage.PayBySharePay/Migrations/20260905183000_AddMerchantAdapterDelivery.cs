using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataStorage.PayBySharePay.Migrations
{
    public partial class AddMerchantAdapterDelivery : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MerchantOrderUrl",
                table: "Participants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalOrderNumber",
                table: "MerchantOrders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalResponseJson",
                table: "MerchantOrders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiersJson",
                table: "MerchantOrderItems",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "MerchantOrderUrl", table: "Participants");
            migrationBuilder.DropColumn(name: "ExternalOrderNumber", table: "MerchantOrders");
            migrationBuilder.DropColumn(name: "ExternalResponseJson", table: "MerchantOrders");
            migrationBuilder.DropColumn(name: "ModifiersJson", table: "MerchantOrderItems");
        }
    }
}
