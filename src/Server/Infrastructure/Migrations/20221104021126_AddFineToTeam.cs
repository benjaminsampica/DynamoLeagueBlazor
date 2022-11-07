using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DynamoLeagueBlazor.Server.Infrastructure.Migrations
{
    public partial class AddFineToTeam : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Fines_Players_PlayerId",
                table: "Fines");

            migrationBuilder.AlterColumn<int>(
                name: "PlayerId",
                table: "Fines",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Fines_Players_PlayerId",
                table: "Fines",
                column: "PlayerId",
                principalTable: "Players",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Fines_Players_PlayerId",
                table: "Fines");

            migrationBuilder.AlterColumn<int>(
                name: "PlayerId",
                table: "Fines",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Fines_Players_PlayerId",
                table: "Fines",
                column: "PlayerId",
                principalTable: "Players",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
