using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace cs2price_prediction.Migrations
{
    /// <inheritdoc />
    public partial class InitialCs2Schema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "cs2");

            migrationBuilder.CreateTable(
                name: "doppler_phases",
                schema: "cs2",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_doppler_phases", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "stickers",
                schema: "cs2",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stickers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "weapon_types",
                schema: "cs2",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_weapon_types", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "wear_tiers",
                schema: "cs2",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_wear_tiers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "sticker_prices",
                schema: "cs2",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StickerId = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sticker_prices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_sticker_prices_stickers_StickerId",
                        column: x => x.StickerId,
                        principalSchema: "cs2",
                        principalTable: "stickers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "weapons",
                schema: "cs2",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    WeaponTypeId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_weapons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_weapons_weapon_types_WeaponTypeId",
                        column: x => x.WeaponTypeId,
                        principalSchema: "cs2",
                        principalTable: "weapon_types",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "skins",
                schema: "cs2",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WeaponId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PaintIndex = table.Column<int>(type: "integer", nullable: false),
                    PatternStyle = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_skins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_skins_weapons_WeaponId",
                        column: x => x.WeaponId,
                        principalSchema: "cs2",
                        principalTable: "weapons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "case_hardened_gun_patterns",
                schema: "cs2",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SkinId = table.Column<int>(type: "integer", nullable: false),
                    Pattern = table.Column<int>(type: "integer", nullable: false),
                    PlaysideBlue = table.Column<double>(type: "double precision", nullable: false),
                    BacksideBlue = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_case_hardened_gun_patterns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_case_hardened_gun_patterns_skins_SkinId",
                        column: x => x.SkinId,
                        principalSchema: "cs2",
                        principalTable: "skins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "case_hardened_knife_patterns",
                schema: "cs2",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SkinId = table.Column<int>(type: "integer", nullable: false),
                    Pattern = table.Column<int>(type: "integer", nullable: false),
                    BacksideBlue = table.Column<double>(type: "double precision", nullable: false),
                    BacksidePurple = table.Column<double>(type: "double precision", nullable: true),
                    BacksideGold = table.Column<double>(type: "double precision", nullable: true),
                    PlaysideBlue = table.Column<double>(type: "double precision", nullable: false),
                    PlaysidePurple = table.Column<double>(type: "double precision", nullable: true),
                    PlaysideGold = table.Column<double>(type: "double precision", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_case_hardened_knife_patterns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_case_hardened_knife_patterns_skins_SkinId",
                        column: x => x.SkinId,
                        principalSchema: "cs2",
                        principalTable: "skins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "doppler_skin_phases",
                schema: "cs2",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SkinId = table.Column<int>(type: "integer", nullable: false),
                    PhaseId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_doppler_skin_phases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_doppler_skin_phases_doppler_phases_PhaseId",
                        column: x => x.PhaseId,
                        principalSchema: "cs2",
                        principalTable: "doppler_phases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_doppler_skin_phases_skins_SkinId",
                        column: x => x.SkinId,
                        principalSchema: "cs2",
                        principalTable: "skins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "fade_gun_patterns",
                schema: "cs2",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SkinId = table.Column<int>(type: "integer", nullable: false),
                    Pattern = table.Column<int>(type: "integer", nullable: false),
                    FadeSeed = table.Column<int>(type: "integer", nullable: false),
                    FadePercentage = table.Column<double>(type: "double precision", nullable: false),
                    FadeRank = table.Column<double>(type: "double precision", nullable: false),
                    FadeType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fade_gun_patterns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_fade_gun_patterns_skins_SkinId",
                        column: x => x.SkinId,
                        principalSchema: "cs2",
                        principalTable: "skins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "fade_knife_patterns",
                schema: "cs2",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SkinId = table.Column<int>(type: "integer", nullable: false),
                    Pattern = table.Column<int>(type: "integer", nullable: false),
                    FadePercentage = table.Column<double>(type: "double precision", nullable: false),
                    FadeRank = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fade_knife_patterns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_fade_knife_patterns_skins_SkinId",
                        column: x => x.SkinId,
                        principalSchema: "cs2",
                        principalTable: "skins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_case_hardened_gun_patterns_SkinId_Pattern",
                schema: "cs2",
                table: "case_hardened_gun_patterns",
                columns: new[] { "SkinId", "Pattern" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_case_hardened_knife_patterns_SkinId_Pattern",
                schema: "cs2",
                table: "case_hardened_knife_patterns",
                columns: new[] { "SkinId", "Pattern" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_doppler_phases_Name",
                schema: "cs2",
                table: "doppler_phases",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_doppler_skin_phases_PhaseId",
                schema: "cs2",
                table: "doppler_skin_phases",
                column: "PhaseId");

            migrationBuilder.CreateIndex(
                name: "IX_doppler_skin_phases_SkinId_PhaseId",
                schema: "cs2",
                table: "doppler_skin_phases",
                columns: new[] { "SkinId", "PhaseId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_fade_gun_patterns_SkinId_Pattern",
                schema: "cs2",
                table: "fade_gun_patterns",
                columns: new[] { "SkinId", "Pattern" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_fade_knife_patterns_SkinId_Pattern",
                schema: "cs2",
                table: "fade_knife_patterns",
                columns: new[] { "SkinId", "Pattern" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_skins_WeaponId_Name",
                schema: "cs2",
                table: "skins",
                columns: new[] { "WeaponId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sticker_prices_StickerId",
                schema: "cs2",
                table: "sticker_prices",
                column: "StickerId");

            migrationBuilder.CreateIndex(
                name: "IX_stickers_Name",
                schema: "cs2",
                table: "stickers",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_weapon_types_Code",
                schema: "cs2",
                table: "weapon_types",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_weapons_Name",
                schema: "cs2",
                table: "weapons",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_weapons_WeaponTypeId",
                schema: "cs2",
                table: "weapons",
                column: "WeaponTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_wear_tiers_Name",
                schema: "cs2",
                table: "wear_tiers",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "case_hardened_gun_patterns",
                schema: "cs2");

            migrationBuilder.DropTable(
                name: "case_hardened_knife_patterns",
                schema: "cs2");

            migrationBuilder.DropTable(
                name: "doppler_skin_phases",
                schema: "cs2");

            migrationBuilder.DropTable(
                name: "fade_gun_patterns",
                schema: "cs2");

            migrationBuilder.DropTable(
                name: "fade_knife_patterns",
                schema: "cs2");

            migrationBuilder.DropTable(
                name: "sticker_prices",
                schema: "cs2");

            migrationBuilder.DropTable(
                name: "wear_tiers",
                schema: "cs2");

            migrationBuilder.DropTable(
                name: "doppler_phases",
                schema: "cs2");

            migrationBuilder.DropTable(
                name: "skins",
                schema: "cs2");

            migrationBuilder.DropTable(
                name: "stickers",
                schema: "cs2");

            migrationBuilder.DropTable(
                name: "weapons",
                schema: "cs2");

            migrationBuilder.DropTable(
                name: "weapon_types",
                schema: "cs2");
        }
    }
}
