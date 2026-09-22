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
        // Guard check: skip if database already has doctors seeded
        if (context.Doctors.Any())
        {
            return;
        }

        var seedDate = DateTime.UtcNow;

        // ── 1. Seed Specialties ──────────────────────────────────────────────────
        if (!context.Specialties.Any())
        {
            context.Specialties.AddRange(
                new Specialty { Name = "Cardiology", Description = "Heart and cardiovascular system care", CreatedAt = seedDate, UpdatedAt = seedDate },
                new Specialty { Name = "Neurology", Description = "Brain and nervous system disorders", CreatedAt = seedDate, UpdatedAt = seedDate },
                new Specialty { Name = "Pediatrics", Description = "Medical care for infants, children, and adolescents", CreatedAt = seedDate, UpdatedAt = seedDate },
                new Specialty { Name = "Dermatology", Description = "Skin, hair, and nail treatments", CreatedAt = seedDate, UpdatedAt = seedDate },
                new Specialty { Name = "Orthopedics", Description = "Bones, joints, and muscular care", CreatedAt = seedDate, UpdatedAt = seedDate }
            );
            await context.SaveChangesAsync();
        }

        var cardio = context.Specialties.First(s => s.Name == "Cardiology");
        var neuro = context.Specialties.First(s => s.Name == "Neurology");
        var pedia = context.Specialties.First(s => s.Name == "Pediatrics");

        // ── 2. Seed Users (Doctors & Admin) ───────────────────────────────────────
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
        }

        var docUser1 = new User
        {
            FullName     = "Dr. Sarah Jenkins",
            Email        = "sarah.jenkins@channelcenter.hospital",
            PasswordHash = "hashed_password",
            Role         = UserRole.Doctor,
            CreatedAt    = seedDate,
            UpdatedAt    = seedDate
        };

        var docUser2 = new User
        {
            FullName     = "Dr. Michael Chen",
            Email        = "michael.chen@channelcenter.hospital",
            PasswordHash = "hashed_password",
            Role         = UserRole.Doctor,
            CreatedAt    = seedDate,
            UpdatedAt    = seedDate
        };

        var docUser3 = new User
        {
            FullName     = "Dr. Emily Rodriguez",
            Email        = "emily.rodriguez@channelcenter.hospital",
            PasswordHash = "hashed_password",
            Role         = UserRole.Doctor,
            CreatedAt    = seedDate,
            UpdatedAt    = seedDate
        };

        context.Users.AddRange(docUser1, docUser2, docUser3);
        await context.SaveChangesAsync();

        // ── 3. Seed Doctors (Student 2) ──────────────────────────────────────────
        var doctor1 = new Doctor
        {
            UserId         = docUser1.Id,
            SpecialtyId    = cardio.Id,
            Qualifications = "MD, FACC, Board Certified Cardiologist",
            CreatedAt      = seedDate,
            UpdatedAt      = seedDate
        };

        var doctor2 = new Doctor
        {
            UserId         = docUser2.Id,
            SpecialtyId    = neuro.Id,
            Qualifications = "MBBS, MD (Neurology), PhD",
            CreatedAt      = seedDate,
            UpdatedAt      = seedDate
        };

        var doctor3 = new Doctor
        {
            UserId         = docUser3.Id,
            SpecialtyId    = pedia.Id,
            Qualifications = "MD, FAAP, Specialist Pediatrician",
            CreatedAt      = seedDate,
            UpdatedAt      = seedDate
        };

        context.Doctors.AddRange(doctor1, doctor2, doctor3);
        await context.SaveChangesAsync();

        // ── 4. Seed Consultation Rooms (Student 2) ────────────────────────────────
        var room1 = new ConsultationRoom { RoomName = "Room 101", Floor = "1st Floor - Wing A", IsActive = true, CreatedAt = seedDate, UpdatedAt = seedDate };
        var room2 = new ConsultationRoom { RoomName = "Room 202", Floor = "2nd Floor - Wing B", IsActive = true, CreatedAt = seedDate, UpdatedAt = seedDate };
        var room3 = new ConsultationRoom { RoomName = "Room 305", Floor = "3rd Floor - Wing C", IsActive = true, CreatedAt = seedDate, UpdatedAt = seedDate };
        var room4 = new ConsultationRoom { RoomName = "Room 408", Floor = "4th Floor - Wing D", IsActive = false, CreatedAt = seedDate, UpdatedAt = seedDate };

        context.ConsultationRooms.AddRange(room1, room2, room3, room4);
        await context.SaveChangesAsync();

        // ── 5. Seed Doctor Schedules (Student 2) ──────────────────────────────────
        context.DoctorSchedules.AddRange(
            new DoctorSchedule
            {
                DoctorId    = doctor1.Id,
                RoomId      = room1.Id,
                StartTime   = DateTime.UtcNow.Date.AddHours(9),
                EndTime     = DateTime.UtcNow.Date.AddHours(12),
                MaxPatients = 15,
                CreatedAt   = seedDate,
                UpdatedAt   = seedDate
            },
            new DoctorSchedule
            {
                DoctorId    = doctor2.Id,
                RoomId      = room2.Id,
                StartTime   = DateTime.UtcNow.Date.AddHours(13),
                EndTime     = DateTime.UtcNow.Date.AddHours(16),
                MaxPatients = 12,
                CreatedAt   = seedDate,
                UpdatedAt   = seedDate
            },
            new DoctorSchedule
            {
                DoctorId    = doctor3.Id,
                RoomId      = room3.Id,
                StartTime   = DateTime.UtcNow.Date.AddDays(1).AddHours(10),
                EndTime     = DateTime.UtcNow.Date.AddDays(1).AddHours(14),
                MaxPatients = 20,
                CreatedAt   = seedDate,
                UpdatedAt   = seedDate
            }
        );
        await context.SaveChangesAsync();

        // ── 6. Seed Doctor Leaves (Student 2) ────────────────────────────────────
        context.DoctorLeaves.AddRange(
            new DoctorLeave
            {
                DoctorId  = doctor1.Id,
                StartDate = DateTime.UtcNow.Date.AddDays(5),
                EndDate   = DateTime.UtcNow.Date.AddDays(8),
                Reason    = "Attending International Cardiology Conference",
                Status    = LeaveStatus.Pending,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            },
            new DoctorLeave
            {
                DoctorId  = doctor2.Id,
                StartDate = DateTime.UtcNow.Date.AddDays(12),
                EndDate   = DateTime.UtcNow.Date.AddDays(14),
                Reason    = "Personal Annual Medical Leave",
                Status    = LeaveStatus.Approved,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            }
        );
        await context.SaveChangesAsync();

        // ── 7. Seed Consultations (Student 2) ─────────────────────────────────────
        context.Consultations.AddRange(
            new Consultation
            {
                AppointmentId    = 1001,
                ClinicalNotes    = "Patient presented with mild arrhythmia. EKG and blood work ordered.",
                PrescriptionData = "{\"medications\":[\"Metoprolol 25mg - 1x daily\",\"Aspirin 81mg - 1x daily\"],\"instructions\":\"Follow up in 2 weeks\"}",
                AttendanceStatus = AttendanceStatus.Present,
                CreatedAt        = seedDate,
                UpdatedAt        = seedDate
            },
            new Consultation
            {
                AppointmentId    = 1002,
                ClinicalNotes    = "Patient reported severe migraine with visual aura.",
                PrescriptionData = "{\"medications\":[\"Sumatriptan 50mg - as needed\"],\"instructions\":\"Rest in quiet room\"}",
                AttendanceStatus = AttendanceStatus.Present,
                CreatedAt        = seedDate,
                UpdatedAt        = seedDate
            }
        );
        await context.SaveChangesAsync();

        // ── 8. Seed Sample AgentWorkflows ──────────────────────────────────────────
        if (!context.AgentWorkflows.Any())
        {
            var wf1 = new AgentWorkflow { Objective = "Intake & triage for patient with acute chest pain", Status = WorkflowStatus.PausedForApproval, RequiresHumanApproval = true, CreatedAt = seedDate, UpdatedAt = seedDate };
            var wf2 = new AgentWorkflow { Objective = "Schedule follow-up cardiology consultation for patient #42", Status = WorkflowStatus.PausedForApproval, RequiresHumanApproval = true, CreatedAt = seedDate.AddHours(2), UpdatedAt = seedDate.AddHours(2) };
            context.AgentWorkflows.AddRange(wf1, wf2);
            await context.SaveChangesAsync();
        }
    }
}
