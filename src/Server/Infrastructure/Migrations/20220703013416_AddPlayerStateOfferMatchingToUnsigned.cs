using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DynamoLeagueBlazor.Server.Infrastructure.Migrations
{
    public partial class AddPlayerStateOfferMatchingToUnsigned : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Convert Offer Matches to use state.
            var sql = $@"UPDATE Players
                SET State = 1
                WHERE DATEADD(day, 3, {nameof(Player.EndOfFreeAgency)}) >= getdate()";

            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var sql = @"UPDATE Players
                SET State = NULL
                WHERE STATE = 1";

            migrationBuilder.Sql(sql);
        }
    }
}
