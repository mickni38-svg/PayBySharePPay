using DataStorage.PayBySharePay.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataStorage.PayBySharePay.Migrations;

[DbContext(typeof(PayBySharePayDbContext))]
[Migration("20260903093600_AddParticipantDeliveryAddress")]
public class AddParticipantDeliveryAddress : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Address",
            table: "Participants",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "PostalCode",
            table: "Participants",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "City",
            table: "Participants",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Country",
            table: "Participants",
            type: "nvarchar(max)",
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "Address", table: "Participants");
        migrationBuilder.DropColumn(name: "PostalCode", table: "Participants");
        migrationBuilder.DropColumn(name: "City", table: "Participants");
        migrationBuilder.DropColumn(name: "Country", table: "Participants");
    }
}
