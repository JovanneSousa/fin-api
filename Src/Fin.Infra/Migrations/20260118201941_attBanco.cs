using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fin.Infra.Migrations
{
    /// <inheritdoc />
    public partial class attBanco : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categories_Icon_DefaultIconId",
                table: "Categories");

            migrationBuilder.RenameColumn(
                name: "DefaultIconId",
                table: "Categories",
                newName: "IconId");

            migrationBuilder.RenameIndex(
                name: "IX_Categories_DefaultIconId",
                table: "Categories",
                newName: "IX_Categories_IconId");

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_Icon_IconId",
                table: "Categories",
                column: "IconId",
                principalTable: "Icon",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categories_Icon_IconId",
                table: "Categories");

            migrationBuilder.RenameColumn(
                name: "IconId",
                table: "Categories",
                newName: "DefaultIconId");

            migrationBuilder.RenameIndex(
                name: "IX_Categories_IconId",
                table: "Categories",
                newName: "IX_Categories_DefaultIconId");

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_Icon_DefaultIconId",
                table: "Categories",
                column: "DefaultIconId",
                principalTable: "Icon",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
