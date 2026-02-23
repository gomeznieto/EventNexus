using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventNexus.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateRefreshTokenGuidId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RefreshTokens_Users_UserId1",
                table: "RefreshTokens");

            migrationBuilder.DropIndex(
                name: "IX_RefreshTokens_UserId1",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "RefreshTokens");

            migrationBuilder.Sql("ALTER TABLE \"RefreshTokens\" ALTER COLUMN \"UserId\" TYPE uuid USING \"UserId\"::uuid;");
            migrationBuilder.CreateIndex(
                    name: "IX_RefreshTokens_UserId",
                    table: "RefreshTokens",
                    column: "UserId");

            migrationBuilder.AddForeignKey(
                    name: "FK_RefreshTokens_Users_UserId",
                    table: "RefreshTokens",
                    column: "UserId",
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                    name: "FK_RefreshTokens_Users_UserId",
                    table: "RefreshTokens");

            migrationBuilder.DropIndex(
                    name: "IX_RefreshTokens_UserId",
                    table: "RefreshTokens");

            migrationBuilder.AlterColumn<string>(
                    name: "UserId",
                    table: "RefreshTokens",
                    type: "text",
                    nullable: false,
                    oldClrType: typeof(Guid),
                    oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                    name: "UserId1",
                    table: "RefreshTokens",
                    type: "uuid",
                    nullable: false,
                    defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                    name: "IX_RefreshTokens_UserId1",
                    table: "RefreshTokens",
                    column: "UserId1");

            migrationBuilder.AddForeignKey(
                    name: "FK_RefreshTokens_Users_UserId1",
                    table: "RefreshTokens",
                    column: "UserId1",
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
        }
    }
}
