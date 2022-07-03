using DynamoLeagueBlazor.Server.Models;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DynamoLeagueBlazor.Server.Infrastructure.Migrations
{
    public partial class AddPlayerState : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "State",
                table: "Players",
                type: "int",
                nullable: true);

            // Convert Free Agents to use state.
            var sql = $@"UPDATE Players
                    SET State = 1
                    WHERE {nameof(Player.EndOfFreeAgency)} >= getdate()
                    AND {nameof(Player.YearContractExpires)} < YEAR(getdate())";

            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "State",
                table: "Players");
        }
    }
}
