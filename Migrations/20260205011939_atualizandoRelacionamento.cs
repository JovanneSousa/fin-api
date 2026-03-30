using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace fin_api.Migrations
{
    /// <inheritdoc />
    public partial class atualizandoRelacionamento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "IconId",
                table: "IconeCategoriaUsuarios",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CorId",
                table: "CorCategoriaUsuarios",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_IconeCategoriaUsuarios",
                table: "IconeCategoriaUsuarios",
                columns: new[] { "UserId", "CategoriaId", "IconId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_CorCategoriaUsuarios",
                table: "CorCategoriaUsuarios",
                columns: new[] { "UserId", "CategoriaId", "CorId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_IconeCategoriaUsuarios",
                table: "IconeCategoriaUsuarios");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CorCategoriaUsuarios",
                table: "CorCategoriaUsuarios");

            migrationBuilder.AlterColumn<string>(
                name: "IconId",
                table: "IconeCategoriaUsuarios",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "CorId",
                table: "CorCategoriaUsuarios",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddPrimaryKey(
                name: "PK_IconeCategoriaUsuarios",
                table: "IconeCategoriaUsuarios",
                columns: new[] { "UserId", "CategoriaId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_CorCategoriaUsuarios",
                table: "CorCategoriaUsuarios",
                columns: new[] { "UserId", "CategoriaId" });
        }
    }
}
