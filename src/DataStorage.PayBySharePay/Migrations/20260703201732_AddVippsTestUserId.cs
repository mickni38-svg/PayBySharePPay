using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataStorage.PayBySharePay.Migrations
{
    /// <inheritdoc />
    public partial class AddVippsTestUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "VippsTestUserId",
                table: "Participants",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Participants_VippsTestUserId",
                table: "Participants",
                column: "VippsTestUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Participants_Participants_VippsTestUserId",
                table: "Participants",
                column: "VippsTestUserId",
                principalTable: "Participants",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Participants_Participants_VippsTestUserId",
                table: "Participants");

            migrationBuilder.DropIndex(
                name: "IX_Participants_VippsTestUserId",
                table: "Participants");

            migrationBuilder.DropColumn(
                name: "VippsTestUserId",
                table: "Participants");
        }
    }
}
