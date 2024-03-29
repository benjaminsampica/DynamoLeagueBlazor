﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DynamoLeagueBlazor.Server.Infrastructure.Migrations
{
    public partial class AddOverBid : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsOverBid",
                table: "Bids",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedOn",
                table: "Bids",
                type: "datetimeoffset",
                nullable: true,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            // Migrate existing data.
            migrationBuilder.Sql(@$"
                    UPDATE Bids SET {nameof(Bid.UpdatedOn)} = {nameof(Bid.CreatedOn)}
                ");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedOn",
                table: "Bids",
                nullable: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsOverBid",
                table: "Bids");

            migrationBuilder.DropColumn(
                name: "UpdatedOn",
                table: "Bids");
        }
    }
}
