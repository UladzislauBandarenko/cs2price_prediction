using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace cs2price_prediction.Migrations
{
    /// <inheritdoc />
    public partial class AddSkinWearTiers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaintIndex",
                schema: "cs2",
                table: "skins");

            migrationBuilder.CreateTable(
                name: "skin_wear_tiers",
                schema: "cs2",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SkinId = table.Column<int>(type: "integer", nullable: false),
                    WearTierId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_skin_wear_tiers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_skin_wear_tiers_skins_SkinId",
                        column: x => x.SkinId,
                        principalSchema: "cs2",
                        principalTable: "skins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_skin_wear_tiers_wear_tiers_WearTierId",
                        column: x => x.WearTierId,
                        principalSchema: "cs2",
                        principalTable: "wear_tiers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_skin_wear_tiers_SkinId_WearTierId",
                schema: "cs2",
                table: "skin_wear_tiers",
                columns: new[] { "SkinId", "WearTierId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_skin_wear_tiers_WearTierId",
                schema: "cs2",
                table: "skin_wear_tiers",
                column: "WearTierId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "skin_wear_tiers",
                schema: "cs2");

            migrationBuilder.AddColumn<int>(
                name: "PaintIndex",
                schema: "cs2",
                table: "skins",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
