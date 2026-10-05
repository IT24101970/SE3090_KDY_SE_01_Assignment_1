using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChannelCenter.API.Migrations
{
    /// <inheritdoc />
    public partial class triageAssessmentupdate2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Referrals_Specialties_TargetSpecialtyId",
                table: "Referrals");

            migrationBuilder.DropForeignKey(
                name: "FK_TriageAssessments_PreConsultationQuestionnaires_Questionnai~",
                table: "TriageAssessments");

            migrationBuilder.DropForeignKey(
                name: "FK_TriageAssessments_Specialties_RecommendedSpecialtyId",
                table: "TriageAssessments");

            migrationBuilder.DropIndex(
                name: "IX_TriageAssessments_QuestionnaireId",
                table: "TriageAssessments");

            migrationBuilder.DropIndex(
                name: "IX_Referrals_TargetSpecialtyId",
                table: "Referrals");

            migrationBuilder.DropColumn(
                name: "PatientId",
                table: "TriageAssessments");

            migrationBuilder.DropColumn(
                name: "QuestionnaireId",
                table: "TriageAssessments");

            migrationBuilder.DropColumn(
                name: "TargetSpecialtyId",
                table: "Referrals");

            migrationBuilder.RenameColumn(
                name: "RecommendedSpecialtyId",
                table: "TriageAssessments",
                newName: "AppointmentId");

            migrationBuilder.RenameIndex(
                name: "IX_TriageAssessments_RecommendedSpecialtyId",
                table: "TriageAssessments",
                newName: "IX_TriageAssessments_AppointmentId");

            migrationBuilder.AddColumn<string>(
                name: "RecommendedSpecialty",
                table: "TriageAssessments",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TargetSpecialty",
                table: "Referrals",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_TriageAssessments_Appointments_AppointmentId",
                table: "TriageAssessments",
                column: "AppointmentId",
                principalTable: "Appointments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TriageAssessments_Appointments_AppointmentId",
                table: "TriageAssessments");

            migrationBuilder.DropColumn(
                name: "RecommendedSpecialty",
                table: "TriageAssessments");

            migrationBuilder.DropColumn(
                name: "TargetSpecialty",
                table: "Referrals");

            migrationBuilder.RenameColumn(
                name: "AppointmentId",
                table: "TriageAssessments",
                newName: "RecommendedSpecialtyId");

            migrationBuilder.RenameIndex(
                name: "IX_TriageAssessments_AppointmentId",
                table: "TriageAssessments",
                newName: "IX_TriageAssessments_RecommendedSpecialtyId");

            migrationBuilder.AddColumn<int>(
                name: "PatientId",
                table: "TriageAssessments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "QuestionnaireId",
                table: "TriageAssessments",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TargetSpecialtyId",
                table: "Referrals",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_TriageAssessments_QuestionnaireId",
                table: "TriageAssessments",
                column: "QuestionnaireId");

            migrationBuilder.CreateIndex(
                name: "IX_Referrals_TargetSpecialtyId",
                table: "Referrals",
                column: "TargetSpecialtyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Referrals_Specialties_TargetSpecialtyId",
                table: "Referrals",
                column: "TargetSpecialtyId",
                principalTable: "Specialties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TriageAssessments_PreConsultationQuestionnaires_Questionnai~",
                table: "TriageAssessments",
                column: "QuestionnaireId",
                principalTable: "PreConsultationQuestionnaires",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TriageAssessments_Specialties_RecommendedSpecialtyId",
                table: "TriageAssessments",
                column: "RecommendedSpecialtyId",
                principalTable: "Specialties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
