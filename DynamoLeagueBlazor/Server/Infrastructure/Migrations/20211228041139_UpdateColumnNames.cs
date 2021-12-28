using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DynamoLeagueBlazor.Server.Infrastructure.Migrations
{
    public partial class UpdateColumnNames : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TeamName",
                table: "Teams",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "TeamLogoUrl",
                table: "Teams",
                newName: "LogoUrl");

            migrationBuilder.RenameColumn(
                name: "HeadShot",
                table: "Players",
                newName: "HeadShotUrl");

            migrationBuilder.RenameColumn(
                name: "ContractLength",
                table: "Players",
                newName: "YearContractExpires");

            migrationBuilder.RenameColumn(
                name: "FineReason",
                table: "Fines",
                newName: "Reason");

            migrationBuilder.RenameColumn(
                name: "FineDate",
                table: "Fines",
                newName: "CreatedOn");

            migrationBuilder.RenameColumn(
                name: "FineAmount",
                table: "Fines",
                newName: "Amount");

            migrationBuilder.RenameColumn(
                name: "DateTime",
                table: "Bids",
                newName: "CreatedOn");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Teams",
                newName: "TeamName");

            migrationBuilder.RenameColumn(
                name: "LogoUrl",
                table: "Teams",
                newName: "TeamLogoUrl");

            migrationBuilder.RenameColumn(
                name: "YearContractExpires",
                table: "Players",
                newName: "ContractLength");

            migrationBuilder.RenameColumn(
                name: "HeadShotUrl",
                table: "Players",
                newName: "HeadShot");

            migrationBuilder.RenameColumn(
                name: "Reason",
                table: "Fines",
                newName: "FineReason");

            migrationBuilder.RenameColumn(
                name: "CreatedOn",
                table: "Fines",
                newName: "FineDate");

            migrationBuilder.RenameColumn(
                name: "Amount",
                table: "Fines",
                newName: "FineAmount");

            migrationBuilder.RenameColumn(
                name: "CreatedOn",
                table: "Bids",
                newName: "DateTime");
        }
    }
}
