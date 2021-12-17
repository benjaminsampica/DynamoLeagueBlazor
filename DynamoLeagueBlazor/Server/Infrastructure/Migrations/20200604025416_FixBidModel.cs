using Microsoft.EntityFrameworkCore.Migrations;

namespace DynamoLeague.Business.Data.Migrations
{
    public partial class FixBidModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bids_Teams_TeamId",
                table: "Bids");

            migrationBuilder.DropColumn(
                name: "TeamKey",
                table: "Bids");

            migrationBuilder.AlterColumn<int>(
                name: "TeamId",
                table: "Bids",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Bids_Teams_TeamId",
                table: "Bids",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "TeamId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bids_Teams_TeamId",
                table: "Bids");

            migrationBuilder.AlterColumn<int>(
                name: "TeamId",
                table: "Bids",
                type: "int",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddColumn<string>(
                name: "TeamKey",
                table: "Bids",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Bids_Teams_TeamId",
                table: "Bids",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "TeamId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
