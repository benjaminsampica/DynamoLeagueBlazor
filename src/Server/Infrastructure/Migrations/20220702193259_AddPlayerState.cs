using DynamoLeagueBlazor.Server.Models;
using Microsoft.EntityFrameworkCore.Migrations;
using System.Text;

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
            var sql = new StringBuilder();
            sql.AppendLine("UPDATE Players")
                .AppendLine("SET State = 0")
                .AppendLine($"WHERE {nameof(Player.EndOfFreeAgency)} >= getdate()")
                .AppendLine($"AND {nameof(Player.YearContractExpires)} < YEAR(getdate()))");

            migrationBuilder.Sql(sql.ToString());
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "State",
                table: "Players");
        }
    }
}
