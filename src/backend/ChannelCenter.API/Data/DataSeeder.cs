using ChannelCenter.API.Models;

namespace ChannelCenter.API.Data;

/// <summary>
/// Seeds initial reference and test data into the database at startup (dev/staging only).
/// Called from Program.cs. NOT used in unit tests.
/// </summary>
public static class DataSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // Only seed if the database has no workflows yet (idempotent guard)
        if (context.AgentWorkflows.Any())
        {
            return;
        }

        var seedDate = new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc);

        // ── Admin User ──────────────────────────────────────────────────────
        if (!context.Users.Any(u => u.Email == "admin@channelcenter.hospital"))
        {
            context.Users.Add(new User
            {
                FullName     = "Chief Admin",
                Email        = "admin@channelcenter.hospital",
                PasswordHash = "$2a$11$rBvpQU1g1F9LXGkVE5FXnuYPrFYjJoqGP3mQmW7dTb2qC0Kj1Y2Za",
                Role         = UserRole.Admin,
                CreatedAt    = seedDate,
                UpdatedAt    = seedDate
            });
            await context.SaveChangesAsync();
        }

        // ── Sample AgentWorkflows ────────────────────────────────────────────
        var wf1 = new AgentWorkflow
        {
            Objective             = "Intake & triage for patient with acute chest pain",
            Status                = WorkflowStatus.PausedForApproval,
            RequiresHumanApproval = true,
            CreatedAt             = seedDate,
            UpdatedAt             = seedDate
        };
        var wf2 = new AgentWorkflow
        {
            Objective             = "Schedule follow-up cardiology consultation for patient #42",
            Status                = WorkflowStatus.PausedForApproval,
            RequiresHumanApproval = true,
            CreatedAt             = seedDate.AddHours(2),
            UpdatedAt             = seedDate.AddHours(2)
        };
        var wf3 = new AgentWorkflow
        {
            Objective             = "Routine intake for patient with seasonal allergies",
            Status                = WorkflowStatus.Running,
            RequiresHumanApproval = false,
            CreatedAt             = seedDate.AddHours(4),
            UpdatedAt             = seedDate.AddHours(4)
        };
        var wf4 = new AgentWorkflow
        {
            Objective             = "Dermatology slot booking – completed successfully",
            Status                = WorkflowStatus.Completed,
            RequiresHumanApproval = false,
            CreatedAt             = seedDate.AddDays(-1),
            UpdatedAt             = seedDate.AddDays(-1).AddHours(1)
        };

        context.AgentWorkflows.AddRange(wf1, wf2, wf3, wf4);
        await context.SaveChangesAsync();

        // ── Realistic AuditLogs ──────────────────────────────────────────────
        context.AuditLogs.AddRange(
            // Workflow 1: Emergency chest pain – IntakeAgent parses symptoms
            new AuditLog { WorkflowId = wf1.Id, AgentName = "IntakeAgent",   ToolCalled = "ParseSymptoms",          ToolOutput = "{\"symptoms\":[\"chest pain\",\"shortness of breath\",\"left arm numbness\"],\"severity\":\"high\",\"duration\":\"2 hours\"}",          CreatedAt = seedDate.AddMinutes(1), UpdatedAt = seedDate.AddMinutes(1) },
            new AuditLog { WorkflowId = wf1.Id, AgentName = "TriageAgent",   ToolCalled = "AssignUrgency",          ToolOutput = "{\"urgencyLevel\":\"Emergency\",\"urgencyScore\":95,\"recommendedSpecialty\":\"Cardiology\",\"reasoning\":\"Chest pain with radiating arm numbness indicates possible MI\"}",   CreatedAt = seedDate.AddMinutes(2), UpdatedAt = seedDate.AddMinutes(2) },
            new AuditLog { WorkflowId = wf1.Id, AgentName = "SafetyAuditor", ToolCalled = "SafetyAuditor_PauseAction", ToolOutput = "{\"action\":\"SafetyAuditor_PauseAction\",\"workflowId\":0,\"reason\":\"Emergency-level urgency detected – mandatory human review required\",\"violation\":\"UrgencyLevel=Emergency triggers mandatory pause policy\"}", CreatedAt = seedDate.AddMinutes(3), UpdatedAt = seedDate.AddMinutes(3) },

            // Workflow 2: Schedule Agent + specialty mismatch
            new AuditLog { WorkflowId = wf2.Id, AgentName = "ScheduleAgent", ToolCalled = "FindAvailableSlot",      ToolOutput = "{\"doctorId\":3,\"doctorName\":\"Dr. Perera\",\"specialty\":\"Cardiology\",\"proposedSlot\":\"2026-09-10T09:00:00Z\",\"roomId\":2}",                                                         CreatedAt = seedDate.AddHours(2).AddMinutes(1), UpdatedAt = seedDate.AddHours(2).AddMinutes(1) },
            new AuditLog { WorkflowId = wf2.Id, AgentName = "SafetyAuditor", ToolCalled = "SafetyAuditor_PauseAction", ToolOutput = "{\"action\":\"SafetyAuditor_PauseAction\",\"workflowId\":0,\"reason\":\"Doctor specialty mismatch\",\"violation\":\"VerifySpecialtyMatch failed for DoctorId=3\"}",                CreatedAt = seedDate.AddHours(2).AddMinutes(2), UpdatedAt = seedDate.AddHours(2).AddMinutes(2) },

            // Workflow 4: Completed dermatology – full clean trace
            new AuditLog { WorkflowId = wf4.Id, AgentName = "IntakeAgent",   ToolCalled = "ParseSymptoms",          ToolOutput = "{\"symptoms\":[\"skin rash\",\"itching\"],\"severity\":\"low\",\"duration\":\"5 days\"}",                                                                                               CreatedAt = seedDate.AddDays(-1).AddMinutes(1), UpdatedAt = seedDate.AddDays(-1).AddMinutes(1) },
            new AuditLog { WorkflowId = wf4.Id, AgentName = "TriageAgent",   ToolCalled = "AssignUrgency",          ToolOutput = "{\"urgencyLevel\":\"Low\",\"urgencyScore\":15,\"recommendedSpecialty\":\"Dermatology\",\"reasoning\":\"Non-emergency skin condition\"}",                                                 CreatedAt = seedDate.AddDays(-1).AddMinutes(2), UpdatedAt = seedDate.AddDays(-1).AddMinutes(2) },
            new AuditLog { WorkflowId = wf4.Id, AgentName = "ScheduleAgent", ToolCalled = "ConfirmBooking",         ToolOutput = "{\"appointmentId\":1,\"doctorId\":5,\"slot\":\"2026-09-02T14:00:00Z\",\"status\":\"Confirmed\"}",                                                                                       CreatedAt = seedDate.AddDays(-1).AddMinutes(3), UpdatedAt = seedDate.AddDays(-1).AddMinutes(3) }
        );

        // ── Historical Admin Approval ────────────────────────────────────────
        var admin = context.Users.FirstOrDefault(u => u.Role == UserRole.Admin);
        if (admin != null)
        {
            context.AdminApprovals.Add(new AdminApproval
            {
                WorkflowId  = wf4.Id,
                AdminUserId = admin.Id,
                Decision    = ApprovalDecision.Approved,
                CreatedAt   = seedDate.AddDays(-1).AddMinutes(5),
                UpdatedAt   = seedDate.AddDays(-1).AddMinutes(5)
            });
        }

        await context.SaveChangesAsync();
    }
}
