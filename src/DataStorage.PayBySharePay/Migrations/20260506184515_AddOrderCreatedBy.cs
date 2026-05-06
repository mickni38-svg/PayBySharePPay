using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataStorage.PayBySharePay.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderCreatedBy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreatedByParticipantId",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CreatedByParticipantId",
                table: "Orders",
                column: "CreatedByParticipantId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Participants_CreatedByParticipantId",
                table: "Orders",
                column: "CreatedByParticipantId",
                principalTable: "Participants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Participants_CreatedByParticipantId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_CreatedByParticipantId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CreatedByParticipantId",
                table: "Orders");
        }
    }
}
