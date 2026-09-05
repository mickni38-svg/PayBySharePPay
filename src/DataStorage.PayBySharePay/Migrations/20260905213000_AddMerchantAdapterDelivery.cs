using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataStorage.PayBySharePay.Migrations;

public partial class AddMerchantAdapterDelivery : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>("MerchantOrderUrl", "Participants", "nvarchar(max)", nullable: true);
        migrationBuilder.AddColumn<string>("ExternalOrderNumber", "MerchantOrders", "nvarchar(100)", maxLength: 100, nullable: true);
        migrationBuilder.AddColumn<string>("ExternalResponseJson", "MerchantOrders", "nvarchar(max)", nullable: true);
        migrationBuilder.AddColumn<string>("ModifiersJson", "MerchantOrderItems", "nvarchar(max)", nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn("MerchantOrderUrl", "Participants");
        migrationBuilder.DropColumn("ExternalOrderNumber", "MerchantOrders");
        migrationBuilder.DropColumn("ExternalResponseJson", "MerchantOrders");
        migrationBuilder.DropColumn("ModifiersJson", "MerchantOrderItems");
    }
}
