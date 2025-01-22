using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskRegistry.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddUsuarioAsignadoRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Titulo",
                table: "Tareas",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateIndex(
                name: "IX_Tareas_UsuarioAsignadoId",
                table: "Tareas",
                column: "UsuarioAsignadoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tareas_Usuarios_UsuarioAsignadoId",
                table: "Tareas",
                column: "UsuarioAsignadoId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tareas_Usuarios_UsuarioAsignadoId",
                table: "Tareas");

            migrationBuilder.DropIndex(
                name: "IX_Tareas_UsuarioAsignadoId",
                table: "Tareas");

            migrationBuilder.AlterColumn<string>(
                name: "Titulo",
                table: "Tareas",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);
        }
    }
}
