using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fin.Infra.Migrations
{
    /// <inheritdoc />
    public partial class InitialSyncMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "Icon",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Url = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Icon", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Nome = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: true),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    IconId = table.Column<string>(type: "text", nullable: true),
                    CorId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Categories_Cor_CorId",
                        column: x => x.CorId,
                        principalTable: "Cor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Categories_Icon_IconId",
                        column: x => x.IconId,
                        principalTable: "Icon",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CorCategoriaUsuarios",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    CategoriaId = table.Column<string>(type: "text", nullable: false),
                    CorId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CorCategoriaUsuarios", x => new { x.UserId, x.CategoriaId, x.CorId });
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
                        name: "FK_CorCategoriaUsuarios_Usuarios_UserId",
                        column: x => x.UserId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IconeCategoriaUsuarios",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    CategoriaId = table.Column<string>(type: "text", nullable: false),
                    IconId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IconeCategoriaUsuarios", x => new { x.UserId, x.CategoriaId, x.IconId });
                    table.ForeignKey(
                        name: "FK_IconeCategoriaUsuarios_Categories_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IconeCategoriaUsuarios_Icon_IconId",
                        column: x => x.IconId,
                        principalTable: "Icon",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_IconeCategoriaUsuarios_Usuarios_UserId",
                        column: x => x.UserId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Titulo = table.Column<string>(type: "text", nullable: true),
                    Valor = table.Column<decimal>(type: "numeric", nullable: false),
                    CategoriaId = table.Column<string>(type: "text", nullable: true),
                    DataMovimentacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsRecurring = table.Column<bool>(type: "boolean", nullable: false),
                    RecorrenciaType = table.Column<int>(type: "integer", nullable: true),
                    RecorrenciaEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Parcelas = table.Column<int>(type: "integer", nullable: true),
                    ParcelaAtual = table.Column<int>(type: "integer", nullable: true),
                    ParentTransactionId = table.Column<string>(type: "text", nullable: true),
                    UsuarioId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transactions_Categories_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categories",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Transactions_Transactions_ParentTransactionId",
                        column: x => x.ParentTransactionId,
                        principalTable: "Transactions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Transactions_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserHiddenCategories",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    CategoryId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserHiddenCategories", x => new { x.UserId, x.CategoryId });
                    table.ForeignKey(
                        name: "FK_UserHiddenCategories_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserHiddenCategories_Usuarios_UserId",
                        column: x => x.UserId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_CorId",
                table: "Categories",
                column: "CorId");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_IconId",
                table: "Categories",
                column: "IconId");

            migrationBuilder.CreateIndex(
                name: "IX_CorCategoriaUsuarios_CategoriaId",
                table: "CorCategoriaUsuarios",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_CorCategoriaUsuarios_CorId",
                table: "CorCategoriaUsuarios",
                column: "CorId");

            migrationBuilder.CreateIndex(
                name: "IX_IconeCategoriaUsuarios_CategoriaId",
                table: "IconeCategoriaUsuarios",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_IconeCategoriaUsuarios_IconId",
                table: "IconeCategoriaUsuarios",
                column: "IconId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_CategoriaId",
                table: "Transactions",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_ParentTransactionId",
                table: "Transactions",
                column: "ParentTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_UsuarioId",
                table: "Transactions",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_UserHiddenCategories_CategoryId",
                table: "UserHiddenCategories",
                column: "CategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CorCategoriaUsuarios");

            migrationBuilder.DropTable(
                name: "IconeCategoriaUsuarios");

            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: "UserHiddenCategories");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Cor");

            migrationBuilder.DropTable(
                name: "Icon");
        }
    }
}
