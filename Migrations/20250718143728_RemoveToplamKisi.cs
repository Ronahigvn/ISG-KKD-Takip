using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ISGKkdTakip.Migrations
{
    /// <inheritdoc />
    public partial class RemoveToplamKisi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ToplamKisi",
                table: "Raporlar");

            migrationBuilder.CreateTable(
                name: "RaporUygunsuzluklar",
                columns: table => new
                {
                    RaporId = table.Column<int>(type: "integer", nullable: false),
                    UygunsuzlukId = table.Column<int>(type: "integer", nullable: false),
                    UygunsuzlukId1 = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RaporUygunsuzluklar", x => new { x.RaporId, x.UygunsuzlukId });
                    table.ForeignKey(
                        name: "FK_RaporUygunsuzluklar_Raporlar_RaporId",
                        column: x => x.RaporId,
                        principalTable: "Raporlar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RaporUygunsuzluklar_Uygunsuzluklar_UygunsuzlukId",
                        column: x => x.UygunsuzlukId,
                        principalTable: "Uygunsuzluklar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RaporUygunsuzluklar_Uygunsuzluklar_UygunsuzlukId1",
                        column: x => x.UygunsuzlukId1,
                        principalTable: "Uygunsuzluklar",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_RaporUygunsuzluklar_UygunsuzlukId",
                table: "RaporUygunsuzluklar",
                column: "UygunsuzlukId");

            migrationBuilder.CreateIndex(
                name: "IX_RaporUygunsuzluklar_UygunsuzlukId1",
                table: "RaporUygunsuzluklar",
                column: "UygunsuzlukId1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RaporUygunsuzluklar");

            migrationBuilder.AddColumn<int>(
                name: "ToplamKisi",
                table: "Raporlar",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
