using Microsoft.EntityFrameworkCore;
using ChannelCenter.API.Models;

namespace ChannelCenter.API.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
        : base(options) { }
    
    // Student 1: Patient Management & Appointment Lifecycle
    public DbSet<Patient> Patients { get; set; } 
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Appointment> Appointments { get; set; } = null!;
    public DbSet<IntakeAgentLog> IntakeAgentLogs { get; set; } = null!;
    
    // Student 2: Doctor Scheduling & Consultation Management
    public DbSet<Doctor> Doctors { get; set; } = null!;
    public DbSet<ConsultationRoom> ConsultationRooms { get; set; } = null!;
    public DbSet<DoctorSchedule> DoctorSchedules { get; set; } = null!;
    public DbSet<DoctorLeave> DoctorLeaves { get; set; } = null!;
    public DbSet<Consultation> Consultations { get; set; } = null!;

    // 3. Medical Triage & Specialist Matching
    public DbSet<Specialty> Specialties { get; set; }
    public DbSet<PreConsultationQuestionnaire> PreConsultationQuestionnaires { get; set; }
    public DbSet<TriageAssessment> TriageAssessments { get; set; }
    public DbSet<SymptomLog> SymptomLogs { get; set; }
    public DbSet<Referral> Referrals { get; set; }
    
    // Student 4: Admin Overview
    public DbSet<AgentWorkflow> AgentWorkflows { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<AdminApproval> AdminApprovals { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Component 1: IntakeAgentLog JSONB columns
        modelBuilder.Entity<IntakeAgentLog>()
            .Property(i => i.InputPayload)
            .HasColumnType("jsonb");

        modelBuilder.Entity<IntakeAgentLog>()
            .Property(i => i.OutputPayload)
            .HasColumnType("jsonb");

        // Component 2: Consultation JSONB columns
        modelBuilder.Entity<Consultation>()
            .Property(c => c.PrescriptionData)
            .HasColumnType("jsonb");

        // Component 3: PreConsultationQuestionnaire JSONB columns
        modelBuilder.Entity<PreConsultationQuestionnaire>()
            .Property(p => p.ResponsesData)
            .HasColumnType("jsonb");

        // Component 4: AuditLog JSONB columns
        modelBuilder.Entity<AuditLog>()
            .Property(a => a.ToolOutput)
            .HasColumnType("jsonb");

        modelBuilder.Entity<AgentWorkflow>()
            .HasIndex(w => w.CorrelationId);

        modelBuilder.Entity<AgentWorkflow>()
            .HasIndex(w => new { w.Status, w.CreatedAt });

        modelBuilder.Entity<AgentWorkflow>()
            .Property(w => w.CorrelationId)
            .HasMaxLength(128);

        modelBuilder.Entity<AgentWorkflow>()
            .Property(w => w.ContractVersion)
            .HasMaxLength(32);

        modelBuilder.Entity<AuditLog>()
            .Property(a => a.CorrelationId)
            .HasMaxLength(128);

        modelBuilder.Entity<AuditLog>()
            .Property(a => a.ContractVersion)
            .HasMaxLength(32);

        // NOTE: Seed data is managed by DataSeeder.cs (called from Program.cs at startup).
        // Keeping seed data out of HasData prevents it from polluting in-memory test databases.
    }
}