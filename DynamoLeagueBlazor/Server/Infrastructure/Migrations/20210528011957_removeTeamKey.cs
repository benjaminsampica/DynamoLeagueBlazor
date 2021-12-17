using Microsoft.EntityFrameworkCore.Migrations;

namespace DynamoLeague.Business.Data.Migrations
{
    public partial class removeTeamKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlayerKey",
                table: "Players");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PlayerKey",
                table: "Players",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
