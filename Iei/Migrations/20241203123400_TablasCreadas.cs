using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Iei.Migrations
{
    /// <inheritdoc />
    public partial class TablasCreadas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Localidad",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    ProvinciaId = table.Column<int>(type: "integer", nullable: false),
                    Guid = table.Column<Guid>(type: "uuid", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastUpdateUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Localidad", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Localidad_Provincia_ProvinciaId",
                        column: x => x.ProvinciaId,
                        principalTable: "Provincia",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Monumento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    Tipo = table.Column<string>(type: "text", nullable: false),
                    Direccion = table.Column<string>(type: "text", nullable: false),
                    CodigoPostal = table.Column<string>(type: "text", nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: false),
                    Longitud = table.Column<double>(type: "double precision", nullable: false),
                    Latitud = table.Column<double>(type: "double precision", nullable: false),
                    LocalidadId = table.Column<int>(type: "integer", nullable: false),
                    Guid = table.Column<Guid>(type: "uuid", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastUpdateUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Monumento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Monumento_Localidad_LocalidadId",
                        column: x => x.LocalidadId,
                        principalTable: "Localidad",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Localidad_ProvinciaId",
                table: "Localidad",
                column: "ProvinciaId");

            migrationBuilder.CreateIndex(
                name: "IX_Monumento_LocalidadId",
                table: "Monumento",
                column: "LocalidadId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Monumento");

            migrationBuilder.DropTable(
                name: "Localidad");
        }
    }
}
