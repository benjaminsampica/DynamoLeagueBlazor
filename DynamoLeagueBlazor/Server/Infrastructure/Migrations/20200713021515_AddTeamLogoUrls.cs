using Microsoft.EntityFrameworkCore.Migrations;

namespace DynamoLeague.Business.Data.Migrations
{
    public partial class AddTeamLogoUrls : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TeamLogoUrl",
                table: "Teams",
                nullable: true,
                defaultValue: null);

            migrationBuilder.AlterColumn<string>(
                 name: "TeamLogoUrl",
                 table: "Teams",
                 nullable: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TeamLogoUrl",
                table: "Teams");
        }
    }
}
