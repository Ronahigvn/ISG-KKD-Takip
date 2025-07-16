using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ISGKkdTakip.Migrations
{
    /// <inheritdoc />
    public partial class YeniKurulum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Mekanlar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Ad = table.Column<string>(type: "text", nullable: false),
                    Aciklama = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mekanlar", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Uygunsuzluklar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Ad = table.Column<string>(type: "text", nullable: false),
                    Aciklama = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Uygunsuzluklar", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Raporlar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MekanId = table.Column<int>(type: "integer", nullable: false),
                    UygunsuzlukId = table.Column<int>(type: "integer", nullable: false),
                    ToplamKisi = table.Column<int>(type: "integer", nullable: false),
                    EkipmanKullanan = table.Column<int>(type: "integer", nullable: false),
                    Tarih = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Raporlar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Raporlar_Mekanlar_MekanId",
                        column: x => x.MekanId,
                        principalTable: "Mekanlar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Raporlar_Uygunsuzluklar_UygunsuzlukId",
                        column: x => x.UygunsuzlukId,
                        principalTable: "Uygunsuzluklar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Raporlar_MekanId",
                table: "Raporlar",
                column: "MekanId");

            migrationBuilder.CreateIndex(
                name: "IX_Raporlar_UygunsuzlukId",
                table: "Raporlar",
                column: "UygunsuzlukId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Raporlar");

            migrationBuilder.DropTable(
                name: "Mekanlar");

            migrationBuilder.DropTable(
                name: "Uygunsuzluklar");
        }
    }
}
