using DataStorage.PayBySharePay.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataStorage.PayBySharePay.Migrations;

[DbContext(typeof(PayBySharePayDbContext))]
[Migration("20260905150000_AddFinalMerchantOrder")]
public sealed class AddFinalMerchantOrder : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "MerchantOrders",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                SourceOrderId = table.Column<int>(type: "int", nullable: false),
                MerchantParticipantId = table.Column<int>(type: "int", nullable: false),
                PayNSyncOrderNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                HostName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                HostPhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                DeliveryAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                DeliveryPostalCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                DeliveryCity = table.Column<string>(type: "nvarchar(max)", nullable: true),
                DeliveryCountry = table.Column<string>(type: "nvarchar(max)", nullable: true),
                TotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                PaymentStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                PaidAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_MerchantOrders", x => x.Id);
                table.ForeignKey(
                    name: "FK_MerchantOrders_Orders_SourceOrderId",
                    column: x => x.SourceOrderId,
                    principalTable: "Orders",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_MerchantOrders_Participants_MerchantParticipantId",
                    column: x => x.MerchantParticipantId,
                    principalTable: "Participants",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "MerchantOrderItems",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                MerchantOrderId = table.Column<int>(type: "int", nullable: false),
                Sku = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Quantity = table.Column<int>(type: "int", nullable: false),
                UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                LineTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_MerchantOrderItems", x => x.Id);
                table.ForeignKey(
                    name: "FK_MerchantOrderItems_MerchantOrders_MerchantOrderId",
                    column: x => x.MerchantOrderId,
                    principalTable: "MerchantOrders",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_MerchantOrderItems_MerchantOrderId",
            table: "MerchantOrderItems",
            column: "MerchantOrderId");

        migrationBuilder.CreateIndex(
            name: "IX_MerchantOrders_MerchantParticipantId",
            table: "MerchantOrders",
            column: "MerchantParticipantId");

        migrationBuilder.CreateIndex(
            name: "IX_MerchantOrders_PayNSyncOrderNumber",
            table: "MerchantOrders",
            column: "PayNSyncOrderNumber",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_MerchantOrders_SourceOrderId",
            table: "MerchantOrders",
            column: "SourceOrderId",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "MerchantOrderItems");
        migrationBuilder.DropTable(name: "MerchantOrders");
    }
}
