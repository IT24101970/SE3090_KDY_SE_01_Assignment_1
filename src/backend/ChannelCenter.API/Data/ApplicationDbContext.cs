using Microsoft.EntityFrameworkCore;
using ChannelCenter.API.Models;

namespace ChannelCenter.API.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
        : base(options) { }

    // This property tells EF Core to create a 'Patients' table
    public DbSet<Patient> Patients { get; set; } 

    // 3. Medical Triage & Specialist Matching
    public DbSet<Specialty> Specialties { get; set; }
    public DbSet<PreConsultationQuestionnaire> PreConsultationQuestionnaires { get; set; }
    public DbSet<TriageAssessment> TriageAssessments { get; set; }
    public DbSet<SymptomLog> SymptomLogs { get; set; }
    public DbSet<Referral> Referrals { get; set; }
    
    // 4. Admin Overview
    public DbSet<AgentWorkflow> AgentWorkflows { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<AdminApproval> AdminApprovals { get; set; }



    // Student 2: Doctor Scheduling & Consultation Management
    public DbSet<Doctor> Doctors { get; set; } = null!;
    public DbSet<ConsultationRoom> ConsultationRooms { get; set; } = null!;
    public DbSet<DoctorSchedule> DoctorSchedules { get; set; } = null!;
    public DbSet<DoctorLeave> DoctorLeaves { get; set; } = null!;
    public DbSet<Consultation> Consultations { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Explicitly map PrescriptionData to PostgreSQL JSONB column type
        modelBuilder.Entity<Consultation>()
            .Property(c => c.PrescriptionData)
            .HasColumnType("jsonb");
    }
}