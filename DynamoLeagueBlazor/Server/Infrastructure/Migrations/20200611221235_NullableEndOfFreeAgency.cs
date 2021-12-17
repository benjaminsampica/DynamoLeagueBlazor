using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace DynamoLeague.Business.Data.Migrations
{
    public partial class NullableEndOfFreeAgency : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "EndOfFreeAgency",
                table: "Players",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "EndOfFreeAgency",
                table: "Players",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldNullable: true);
        }
    }
}
