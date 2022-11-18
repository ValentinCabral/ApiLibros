using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApiAutores.Migrations
{
    public partial class Bibliotecas : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BibliotecaId",
                table: "Libros",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Bibliotecas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bibliotecas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bibliotecas_AspNetUsers_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Libros_BibliotecaId",
                table: "Libros",
                column: "BibliotecaId");

            migrationBuilder.CreateIndex(
                name: "IX_Bibliotecas_UsuarioId",
                table: "Bibliotecas",
                column: "UsuarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Libros_Bibliotecas_BibliotecaId",
                table: "Libros",
                column: "BibliotecaId",
                principalTable: "Bibliotecas",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Libros_Bibliotecas_BibliotecaId",
                table: "Libros");

            migrationBuilder.DropTable(
                name: "Bibliotecas");

            migrationBuilder.DropIndex(
                name: "IX_Libros_BibliotecaId",
                table: "Libros");

            migrationBuilder.DropColumn(
                name: "BibliotecaId",
                table: "Libros");
        }
    }
}
