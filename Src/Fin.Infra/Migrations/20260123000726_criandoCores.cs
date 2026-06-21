using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fin.Infra.Migrations
{
    /// <inheritdoc />
    public partial class criandoCores : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CorId",
                table: "Categories",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Cor",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Url = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cor", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CorCategoriaUsuarios",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    CategoriaId = table.Column<string>(type: "text", nullable: false),
                    CorId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CorCategoriaUsuarios", x => new { x.UserId, x.CategoriaId });
                    table.ForeignKey(
                        name: "FK_CorCategoriaUsuarios_Categories_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CorCategoriaUsuarios_Cor_CorId",
                        column: x => x.CorId,
                        principalTable: "Cor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CorCategoriaUsuarios_Usuario_UserId",
                        column: x => x.UserId,
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_CorId",
                table: "Categories",
                column: "CorId");

            migrationBuilder.CreateIndex(
                name: "IX_CorCategoriaUsuarios_CategoriaId",
                table: "CorCategoriaUsuarios",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_CorCategoriaUsuarios_CorId",
                table: "CorCategoriaUsuarios",
                column: "CorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_Cor_CorId",
                table: "Categories",
                column: "CorId",
                principalTable: "Cor",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categories_Cor_CorId",
                table: "Categories");

            migrationBuilder.DropTable(
                name: "CorCategoriaUsuarios");

            migrationBuilder.DropTable(
                name: "Cor");

            migrationBuilder.DropIndex(
                name: "IX_Categories_CorId",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "CorId",
                table: "Categories");
        }
    }
}
