using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataStorage.PayBySharePay.Migrations
{
    /// <inheritdoc />
    public partial class AddParticipantPasswordHash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "Participants",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "Participants");
        }
    }
}
