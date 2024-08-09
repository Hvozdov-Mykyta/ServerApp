using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServerApp.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShortenedLinks_Users_shortenedBy",
                table: "ShortenedLinks");

            migrationBuilder.DropIndex(
                name: "IX_ShortenedLinks_shortenedBy",
                table: "ShortenedLinks");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ShortenedLinks_shortenedBy",
                table: "ShortenedLinks",
                column: "shortenedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_ShortenedLinks_Users_shortenedBy",
                table: "ShortenedLinks",
                column: "shortenedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
