using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataStorage.PayBySharePay.Migrations
{
    /// <inheritdoc />
    public partial class AddParticipantTokenAndDraftParticipant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ParticipantToken",
                table: "OrderParticipants",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ParticipantId",
                table: "MerchantOrderDrafts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderParticipants_ParticipantToken",
                table: "OrderParticipants",
                column: "ParticipantToken",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MerchantOrderDrafts_ParticipantId",
                table: "MerchantOrderDrafts",
                column: "ParticipantId");

            migrationBuilder.AddForeignKey(
                name: "FK_MerchantOrderDrafts_Participants_ParticipantId",
                table: "MerchantOrderDrafts",
                column: "ParticipantId",
                principalTable: "Participants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MerchantOrderDrafts_Participants_ParticipantId",
                table: "MerchantOrderDrafts");

            migrationBuilder.DropIndex(
                name: "IX_OrderParticipants_ParticipantToken",
                table: "OrderParticipants");

            migrationBuilder.DropIndex(
                name: "IX_MerchantOrderDrafts_ParticipantId",
                table: "MerchantOrderDrafts");

            migrationBuilder.DropColumn(
                name: "ParticipantToken",
                table: "OrderParticipants");

            migrationBuilder.DropColumn(
                name: "ParticipantId",
                table: "MerchantOrderDrafts");
        }
    }
}
