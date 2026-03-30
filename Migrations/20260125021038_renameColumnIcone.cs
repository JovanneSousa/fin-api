using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace fin_api.Migrations
{
    /// <inheritdoc />
    public partial class renameColumnIcone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.AddForeignKey(
                name: "FK_IconeCategoriaUsuarios_Categories_CategoriaId",
                table: "IconeCategoriaUsuarios",
                column: "CategoriaId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_IconeCategoriaUsuarios_Icon_IconId",
                table: "IconeCategoriaUsuarios",
                column: "IconId",
                principalTable: "Icon",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_IconeCategoriaUsuarios_Usuario_UserId",
                table: "IconeCategoriaUsuarios",
                column: "UserId",
                principalTable: "Usuario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IconeCategoriaUsuarios_Categories_CategoriaId",
                table: "IconeCategoriaUsuarios");

            migrationBuilder.DropForeignKey(
                name: "FK_IconeCategoriaUsuarios_Icon_IconId",
                table: "IconeCategoriaUsuarios");

            migrationBuilder.DropForeignKey(
                name: "FK_IconeCategoriaUsuarios_Usuario_UserId",
                table: "IconeCategoriaUsuarios");

            migrationBuilder.DropPrimaryKey(
                name: "PK_IconeCategoriaUsuarios",
                table: "IconeCategoriaUsuarios");

            migrationBuilder.RenameTable(
                name: "IconeCategoriaUsuarios",
                newName: "CategoriaUsuarios");

            migrationBuilder.RenameIndex(
                name: "IX_IconeCategoriaUsuarios_IconId",
                table: "CategoriaUsuarios",
                newName: "IX_CategoriaUsuarios_IconId");

            migrationBuilder.RenameIndex(
                name: "IX_IconeCategoriaUsuarios_CategoriaId",
                table: "CategoriaUsuarios",
                newName: "IX_CategoriaUsuarios_CategoriaId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CategoriaUsuarios",
                table: "CategoriaUsuarios",
                columns: new[] { "UserId", "CategoriaId" });

            migrationBuilder.AddForeignKey(
                name: "FK_CategoriaUsuarios_Categories_CategoriaId",
                table: "CategoriaUsuarios",
                column: "CategoriaId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CategoriaUsuarios_Icon_IconId",
                table: "CategoriaUsuarios",
                column: "IconId",
                principalTable: "Icon",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_CategoriaUsuarios_Usuario_UserId",
                table: "CategoriaUsuarios",
                column: "UserId",
                principalTable: "Usuario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
