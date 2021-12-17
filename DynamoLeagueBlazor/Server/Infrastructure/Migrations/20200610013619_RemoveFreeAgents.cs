using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace DynamoLeague.Business.Data.Migrations
{
    public partial class RemoveFreeAgents : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bids_FreeAgents_FreeAgentId",
                table: "Bids");

            migrationBuilder.DropTable(
                name: "FreeAgents");

            migrationBuilder.DropIndex(
                name: "IX_Bids_FreeAgentId",
                table: "Bids");

            migrationBuilder.DropColumn(
                name: "FreeAgentId",
                table: "Bids");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndOfFreeAgency",
                table: "Players",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "PlayerId",
                table: "Bids",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Bids_PlayerId",
                table: "Bids",
                column: "PlayerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bids_Players_PlayerId",
                table: "Bids",
                column: "PlayerId",
                principalTable: "Players",
                principalColumn: "PlayerId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bids_Players_PlayerId",
                table: "Bids");

            migrationBuilder.DropIndex(
                name: "IX_Bids_PlayerId",
                table: "Bids");

            migrationBuilder.DropColumn(
                name: "EndOfFreeAgency",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "PlayerId",
                table: "Bids");

            migrationBuilder.AddColumn<int>(
                name: "FreeAgentId",
                table: "Bids",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "FreeAgents",
                columns: table => new
                {
                    FreeAgentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EndOfFreeAgency = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PlayerId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FreeAgents", x => x.FreeAgentId);
                    table.ForeignKey(
                        name: "FK_FreeAgents_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "PlayerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bids_FreeAgentId",
                table: "Bids",
                column: "FreeAgentId");

            migrationBuilder.CreateIndex(
                name: "IX_FreeAgents_PlayerId",
                table: "FreeAgents",
                column: "PlayerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bids_FreeAgents_FreeAgentId",
                table: "Bids",
                column: "FreeAgentId",
                principalTable: "FreeAgents",
                principalColumn: "FreeAgentId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
