using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DynamoLeagueBlazor.Server.Infrastructure.Migrations
{
    public partial class AddPlayerStateUnsignedToRostered : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Convert Unsigned to use state.
            var sql = $@"UPDATE Players
                SET State = 2
                WHERE {nameof(Player.EndOfFreeAgency)} IS NULL
                    AND {nameof(Player.YearContractExpires)} IS NULL
                    AND {nameof(Player.YearAcquired)} = 2022";

            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var sql = @"UPDATE Players
                SET State = NULL
                WHERE STATE = 2";

            migrationBuilder.Sql(sql);
        }
    }
}
