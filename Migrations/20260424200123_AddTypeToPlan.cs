using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APM.API.Migrations
{
    /// <inheritdoc />
    public partial class AddTypeToPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "ActionPlans",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "ActionPlans");
        }
    }
}
