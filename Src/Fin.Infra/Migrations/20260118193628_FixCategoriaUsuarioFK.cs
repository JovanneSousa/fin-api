using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fin.Infra.Migrations
{
    /// <inheritdoc />
    public partial class FixCategoriaUsuarioFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.CreateTable(
                name: "CategoriaUsuarios",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    CategoriaId = table.Column<string>(type: "text", nullable: false),
                    IconId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoriaUsuarios", x => new { x.UserId, x.CategoriaId });
                    table.ForeignKey(
                        name: "FK_CategoriaUsuarios_Categories_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CategoriaUsuarios_Icon_IconId",
                        column: x => x.IconId,
                        principalTable: "Icon",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CategoriaUsuarios_Usuario_UserId",
                        column: x => x.UserId,
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CategoriaUsuarios_CategoriaId",
                table: "CategoriaUsuarios",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_CategoriaUsuarios_IconId",
                table: "CategoriaUsuarios",
                column: "IconId");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CategoriaUsuarios");

            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: "UserHiddenCategories");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Usuario");

            migrationBuilder.DropTable(
                name: "Icon");
        }
    }
}
