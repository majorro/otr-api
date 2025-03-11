﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Database.Migrations
{
    /// <inheritdoc />
    public partial class Tournament_LobbySizeRename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "lobby_size",
                table: "tournaments",
                newName: "lobby_team_size");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "lobby_team_size",
                table: "tournaments",
                newName: "lobby_size");
        }
    }
}
