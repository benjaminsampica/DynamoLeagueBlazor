using DynamoLeagueBlazor.Server.Models;
using Microsoft.EntityFrameworkCore.Migrations;
using System.Text;

#nullable disable

namespace DynamoLeagueBlazor.Server.Infrastructure.Migrations
{
    public partial class AddPlayerStateOfferMatchingToUnsigned : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Convert Offer Matches to use state.
            var sql = new StringBuilder();
            sql.AppendLine("UPDATE Players")
                .AppendLine("SET State = 1")
                .AppendLine($"WHERE DATEADD(day, 3, {nameof(Player.EndOfFreeAgency)}) <= getdate()");

            migrationBuilder.Sql(sql.ToString());
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var sql = new StringBuilder();
            sql.AppendLine("UPDATE Players")
                .AppendLine("SET State = NULL")
                .AppendLine($"WHERE STATE = 1");

            migrationBuilder.Sql(sql.ToString());
        }
    }
}
