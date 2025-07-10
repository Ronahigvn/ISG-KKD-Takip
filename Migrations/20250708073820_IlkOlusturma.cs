using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ISGKkdTakip.Migrations
{
    /// <inheritdoc />
    public partial class IlkOlusturma : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Tip",
                table: "Uygunsuzluklar",
                newName: "Ad");

            migrationBuilder.AddColumn<string>(
                name: "Aciklama",
                table: "Uygunsuzluklar",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Aciklama",
                table: "Mekanlar",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Aciklama",
                table: "Uygunsuzluklar");

            migrationBuilder.DropColumn(
                name: "Aciklama",
                table: "Mekanlar");

            migrationBuilder.RenameColumn(
                name: "Ad",
                table: "Uygunsuzluklar",
                newName: "Tip");
        }
    }
}
