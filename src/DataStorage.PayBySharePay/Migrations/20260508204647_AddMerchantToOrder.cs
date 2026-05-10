using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataStorage.PayBySharePay.Migrations
{
    /// <inheritdoc />
    public partial class AddMerchantToOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GroupOrderUrl",
                table: "Participants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JoinToken",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MerchantParticipantId",
                table: "Orders",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_MerchantParticipantId",
                table: "Orders",
                column: "MerchantParticipantId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Participants_MerchantParticipantId",
                table: "Orders",
                column: "MerchantParticipantId",
                principalTable: "Participants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Participants_MerchantParticipantId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_MerchantParticipantId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "GroupOrderUrl",
                table: "Participants");

            migrationBuilder.DropColumn(
                name: "JoinToken",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "MerchantParticipantId",
                table: "Orders");
        }
    }
}
