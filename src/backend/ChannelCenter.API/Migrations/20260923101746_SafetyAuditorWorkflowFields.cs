using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChannelCenter.API.Migrations
{
    /// <inheritdoc />
    public partial class SafetyAuditorWorkflowFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContractVersion",
                table: "AuditLogs",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CorrelationId",
                table: "AuditLogs",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DurationMs",
                table: "AuditLogs",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Outcome",
                table: "AuditLogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StepName",
                table: "AuditLogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                table: "AgentWorkflows",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContractVersion",
                table: "AgentWorkflows",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "safety-audit.v1");

            migrationBuilder.AddColumn<string>(
                name: "CorrelationId",
                table: "AgentWorkflows",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ErrorCode",
                table: "AgentWorkflows",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ErrorMessage",
                table: "AgentWorkflows",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FinalOutcome",
                table: "AgentWorkflows",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PlanSummary",
                table: "AgentWorkflows",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RiskLevel",
                table: "AgentWorkflows",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SafeFailedAt",
                table: "AgentWorkflows",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SafetyAuditStartedAt",
                table: "AgentWorkflows",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ValidationSummary",
                table: "AgentWorkflows",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AgentWorkflows_CorrelationId",
                table: "AgentWorkflows",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_AgentWorkflows_Status_CreatedAt",
                table: "AgentWorkflows",
                columns: new[] { "Status", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AgentWorkflows_CorrelationId",
                table: "AgentWorkflows");

            migrationBuilder.DropIndex(
                name: "IX_AgentWorkflows_Status_CreatedAt",
                table: "AgentWorkflows");

            migrationBuilder.DropColumn(
                name: "ContractVersion",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "CorrelationId",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "DurationMs",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "Outcome",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "StepName",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "AgentWorkflows");

            migrationBuilder.DropColumn(
                name: "ContractVersion",
                table: "AgentWorkflows");

            migrationBuilder.DropColumn(
                name: "CorrelationId",
                table: "AgentWorkflows");

            migrationBuilder.DropColumn(
                name: "ErrorCode",
                table: "AgentWorkflows");

            migrationBuilder.DropColumn(
                name: "ErrorMessage",
                table: "AgentWorkflows");

            migrationBuilder.DropColumn(
                name: "FinalOutcome",
                table: "AgentWorkflows");

            migrationBuilder.DropColumn(
                name: "PlanSummary",
                table: "AgentWorkflows");

            migrationBuilder.DropColumn(
                name: "RiskLevel",
                table: "AgentWorkflows");

            migrationBuilder.DropColumn(
                name: "SafeFailedAt",
                table: "AgentWorkflows");

            migrationBuilder.DropColumn(
                name: "SafetyAuditStartedAt",
                table: "AgentWorkflows");

            migrationBuilder.DropColumn(
                name: "ValidationSummary",
                table: "AgentWorkflows");
        }
    }
}
