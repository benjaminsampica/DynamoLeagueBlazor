using Microsoft.EntityFrameworkCore.Migrations;

namespace DynamoLeague.Business.Data.Migrations
{
    public partial class DropFineTeamId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Fines_Teams_TeamId",
                table: "Fines");

            migrationBuilder.DropIndex(
                name: "IX_Fines_TeamId",
                table: "Fines");

            migrationBuilder.DropColumn(
                name: "TeamId",
                table: "Fines");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TeamId",
                table: "Fines",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Fines_TeamId",
                table: "Fines",
                column: "TeamId");

            migrationBuilder.AddForeignKey(
                name: "FK_Fines_Teams_TeamId",
                table: "Fines",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "TeamId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
