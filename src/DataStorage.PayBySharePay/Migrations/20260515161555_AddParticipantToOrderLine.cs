using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataStorage.PayBySharePay.Migrations
{
    /// <inheritdoc />
    public partial class AddParticipantToOrderLine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ParticipantId",
                table: "MerchantOrderLines",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MerchantOrderLines_ParticipantId",
                table: "MerchantOrderLines",
                column: "ParticipantId");

            migrationBuilder.AddForeignKey(
                name: "FK_MerchantOrderLines_Participants_ParticipantId",
                table: "MerchantOrderLines",
                column: "ParticipantId",
                principalTable: "Participants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MerchantOrderLines_Participants_ParticipantId",
                table: "MerchantOrderLines");

            migrationBuilder.DropIndex(
                name: "IX_MerchantOrderLines_ParticipantId",
                table: "MerchantOrderLines");

            migrationBuilder.DropColumn(
                name: "ParticipantId",
                table: "MerchantOrderLines");
        }
    }
}
