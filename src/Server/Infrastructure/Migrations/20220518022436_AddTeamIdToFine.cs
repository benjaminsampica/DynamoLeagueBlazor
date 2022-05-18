﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DynamoLeagueBlazor.Server.Infrastructure.Migrations
{
    public partial class AddTeamIdToFine : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TeamId",
                table: "Fines",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Fines_TeamId",
                table: "Fines",
                column: "TeamId");

            migrationBuilder.AddForeignKey(
                name: "FK_Fines_Teams_TeamId",
                table: "Fines",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Fines_Teams_TeamId",
                table: "Fines");

            migrationBuilder.DropIndex(
                name: "IX_Fines_TeamId",
                table: "Fines");

            migrationBuilder.DropColumn(
                name: "TeamId",
                table: "Fines");
        }
    }
}
