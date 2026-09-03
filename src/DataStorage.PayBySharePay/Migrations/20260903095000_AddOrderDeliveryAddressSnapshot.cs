using DataStorage.PayBySharePay.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataStorage.PayBySharePay.Migrations;

[DbContext(typeof(PayBySharePayDbContext))]
[Migration("20260903095000_AddOrderDeliveryAddressSnapshot")]
public class AddOrderDeliveryAddressSnapshot : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "DeliveryAddress",
            table: "Orders",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "DeliveryPostalCode",
            table: "Orders",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "DeliveryCity",
            table: "Orders",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "DeliveryCountry",
            table: "Orders",
            type: "nvarchar(max)",
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "DeliveryAddress", table: "Orders");
        migrationBuilder.DropColumn(name: "DeliveryPostalCode", table: "Orders");
        migrationBuilder.DropColumn(name: "DeliveryCity", table: "Orders");
        migrationBuilder.DropColumn(name: "DeliveryCountry", table: "Orders");
    }
}
