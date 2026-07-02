using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataStorage.PayBySharePay.Migrations
{
    /// <inheritdoc />
    public partial class AddMerchantVippsCredentials : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VippsClientId",
                table: "Participants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VippsClientSecret",
                table: "Participants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VippsSubscriptionKey",
                table: "Participants",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VippsClientId",
                table: "Participants");

            migrationBuilder.DropColumn(
                name: "VippsClientSecret",
                table: "Participants");

            migrationBuilder.DropColumn(
                name: "VippsSubscriptionKey",
                table: "Participants");
        }
    }
}
