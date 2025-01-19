using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SomeCatIDK.PirateJim.Migrations
{
    /// <inheritdoc />
    public partial class AutoMessageChannels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LastMessageChannels",
                columns: table => new
                {
                    ChannelId = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MessageId = table.Column<ulong>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LastMessageChannels", x => x.ChannelId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LastMessageChannels");
        }
    }
}
