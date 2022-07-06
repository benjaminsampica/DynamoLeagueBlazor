using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DynamoLeagueBlazor.Server.Infrastructure.Migrations
{
    public partial class AddPlayerStateRosteredToFreeAgent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Convert Signed to use state.
            var sql = $@"UPDATE Players
                SET State = 3
                WHERE Rostered = true";

            migrationBuilder.Sql(sql.ToString());

            migrationBuilder.DropColumn(
                name: "Rostered",
                table: "Players");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Rostered",
                table: "Players",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
