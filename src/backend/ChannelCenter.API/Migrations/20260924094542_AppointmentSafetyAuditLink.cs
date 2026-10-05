using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChannelCenter.API.Migrations
{
    /// <inheritdoc />
    public partial class AppointmentSafetyAuditLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AppointmentId",
                table: "AgentWorkflows",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AgentWorkflows_AppointmentId",
                table: "AgentWorkflows",
                column: "AppointmentId",
                unique: true,
                filter: "\"AppointmentId\" IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_AgentWorkflows_Appointments_AppointmentId",
                table: "AgentWorkflows",
                column: "AppointmentId",
                principalTable: "Appointments",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AgentWorkflows_Appointments_AppointmentId",
                table: "AgentWorkflows");

            migrationBuilder.DropIndex(
                name: "IX_AgentWorkflows_AppointmentId",
                table: "AgentWorkflows");

            migrationBuilder.DropColumn(
                name: "AppointmentId",
                table: "AgentWorkflows");
        }
    }
}
