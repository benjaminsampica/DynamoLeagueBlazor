using Microsoft.EntityFrameworkCore.Migrations;

namespace DynamoLeague.Business.Data.Migrations
{
    public partial class TeamsTableMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TeamKey",
                table: "Teams");
        }
    }
}
