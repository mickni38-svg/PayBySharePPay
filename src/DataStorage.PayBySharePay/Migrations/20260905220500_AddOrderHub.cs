using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataStorage.PayBySharePay.Migrations;

public partial class AddOrderHub : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "OrderHubEnabled",
            table: "Participants",
            type: "bit",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<string>(
            name: "Note",
            table: "MerchantOrders",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "OrderHubStatus",
            table: "MerchantOrders",
            type: "nvarchar(30)",
            maxLength: 30,
            nullable: false,
            defaultValue: "New");

        migrationBuilder.AddColumn<DateTime>(
            name: "UpdatedAtUtc",
            table: "MerchantOrders",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "OrderHubEnabled", table: "Participants");
        migrationBuilder.DropColumn(name: "Note", table: "MerchantOrders");
        migrationBuilder.DropColumn(name: "OrderHubStatus", table: "MerchantOrders");
        migrationBuilder.DropColumn(name: "UpdatedAtUtc", table: "MerchantOrders");
    }
}
