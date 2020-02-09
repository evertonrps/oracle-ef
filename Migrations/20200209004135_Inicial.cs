using Microsoft.EntityFrameworkCore.Migrations;
using Oracle.EntityFrameworkCore.Metadata;

namespace Oracle_EF.Migrations
{
    public partial class Inicial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BLOG",
                columns: table => new
                {
                    BLOGID = table.Column<int>(nullable: false)
                        .Annotation("Oracle:ValueGenerationStrategy", OracleValueGenerationStrategy.IdentityColumn),
                    URL = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BLOG", x => x.BLOGID);
                });

            migrationBuilder.CreateTable(
                name: "POSTS",
                columns: table => new
                {
                    POSTID = table.Column<int>(nullable: false)
                        .Annotation("Oracle:ValueGenerationStrategy", OracleValueGenerationStrategy.IdentityColumn),
                    TITLE = table.Column<string>(nullable: true),
                    CONTENT = table.Column<string>(nullable: true),
                    BLOGID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_POSTS", x => x.POSTID);
                    table.ForeignKey(
                        name: "FK_POSTS_BLOG_BLOGID",
                        column: x => x.BLOGID,
                        principalTable: "BLOG",
                        principalColumn: "BLOGID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_POSTS_BLOGID",
                table: "POSTS",
                column: "BLOGID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "POSTS");

            migrationBuilder.DropTable(
                name: "BLOG");
        }
    }
}
