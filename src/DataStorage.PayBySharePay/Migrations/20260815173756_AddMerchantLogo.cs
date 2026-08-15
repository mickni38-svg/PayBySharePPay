using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataStorage.PayBySharePay.Migrations
{
    /// <inheritdoc />
    public partial class AddMerchantLogo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LogoContentType",
                table: "Participants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LogoFileName",
                table: "Participants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "LogoImageData",
                table: "Participants",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LogoUpdatedAtUtc",
                table: "Participants",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LogoContentType",
                table: "Participants");

            migrationBuilder.DropColumn(
                name: "LogoFileName",
                table: "Participants");

            migrationBuilder.DropColumn(
                name: "LogoImageData",
                table: "Participants");

            migrationBuilder.DropColumn(
                name: "LogoUpdatedAtUtc",
                table: "Participants");
        }
    }
}
