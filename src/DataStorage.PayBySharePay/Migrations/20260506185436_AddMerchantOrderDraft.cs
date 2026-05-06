using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataStorage.PayBySharePay.Migrations
{
    /// <inheritdoc />
    public partial class AddMerchantOrderDraft : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MerchantOrderDrafts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    MerchantParticipantId = table.Column<int>(type: "int", nullable: false),
                    MerchantDraftReference = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubtotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PaymentMode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MerchantOrderDrafts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MerchantOrderDrafts_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MerchantOrderDrafts_Participants_MerchantParticipantId",
                        column: x => x.MerchantParticipantId,
                        principalTable: "Participants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MerchantOrderLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MerchantOrderDraftId = table.Column<int>(type: "int", nullable: false),
                    LineId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    LineTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MerchantOrderLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MerchantOrderLines_MerchantOrderDrafts_MerchantOrderDraftId",
                        column: x => x.MerchantOrderDraftId,
                        principalTable: "MerchantOrderDrafts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MerchantOrderDrafts_MerchantParticipantId",
                table: "MerchantOrderDrafts",
                column: "MerchantParticipantId");

            migrationBuilder.CreateIndex(
                name: "IX_MerchantOrderDrafts_OrderId",
                table: "MerchantOrderDrafts",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_MerchantOrderLines_MerchantOrderDraftId",
                table: "MerchantOrderLines",
                column: "MerchantOrderDraftId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MerchantOrderLines");

            migrationBuilder.DropTable(
                name: "MerchantOrderDrafts");
        }
    }
}
