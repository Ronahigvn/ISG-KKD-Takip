using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ISGKkdTakip.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Mekanlar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Ad = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mekanlar", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Uygunsuzluklar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Tip = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Uygunsuzluklar", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Raporlar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MekanId = table.Column<int>(type: "INTEGER", nullable: false),
                    UygunsuzlukId = table.Column<int>(type: "INTEGER", nullable: false),
                    ToplamKisi = table.Column<int>(type: "INTEGER", nullable: false),
                    EkipmanKullanan = table.Column<int>(type: "INTEGER", nullable: false),
                    Tarih = table.Column<DateTime>(type: "TEXT", nullable: false)
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
