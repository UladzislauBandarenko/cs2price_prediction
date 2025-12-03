using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cs2price_prediction.Migrations
{
    /// <inheritdoc />
    public partial class RemoveFadeSeedFromFadeGunPatterns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FadeSeed",
                schema: "cs2",
                table: "fade_gun_patterns");

            migrationBuilder.DropColumn(
                name: "FadeType",
                schema: "cs2",
                table: "fade_gun_patterns");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FadeSeed",
                schema: "cs2",
                table: "fade_gun_patterns",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "FadeType",
                schema: "cs2",
                table: "fade_gun_patterns",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }
    }
}
