using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace microserv2_fatu.Migrations
{
public partial class AddDescricaoAItensNotasFiscais : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Descricao",
                table: "ItensNotasFiscais",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Descricao",
                table: "ItensNotasFiscais");
        }
    }
}
