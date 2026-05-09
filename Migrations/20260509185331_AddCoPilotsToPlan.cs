using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APM.API.Migrations
{
    /// <inheritdoc />
    public partial class AddCoPilotsToPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ActionPlanCoPilots",
                columns: table => new
                {
                    CoManagedPlansId = table.Column<int>(type: "int", nullable: false),
                    CoPilotsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActionPlanCoPilots", x => new { x.CoManagedPlansId, x.CoPilotsId });
                    table.ForeignKey(
                        name: "FK_ActionPlanCoPilots_ActionPlans_CoManagedPlansId",
                        column: x => x.CoManagedPlansId,
                        principalTable: "ActionPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ActionPlanCoPilots_Users_CoPilotsId",
                        column: x => x.CoPilotsId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActionPlanCoPilots_CoPilotsId",
                table: "ActionPlanCoPilots",
                column: "CoPilotsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActionPlanCoPilots");
        }
    }
}
