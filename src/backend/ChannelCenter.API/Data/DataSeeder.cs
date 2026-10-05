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

        // ── 9. Seed Sample Patients, Appointments, Triage Assessments & Referrals ──
        if (!context.Patients.Any())
        {
            var p1 = new Patient { Name = "Alice Perera", NIC = "199245102930", Gender = "Female", PhoneNumber = "+94771234567", DateOfBirth = new DateTime(1992, 5, 14), BloodGroup = "A+", CreatedAt = seedDate, UpdatedAt = seedDate };
            var p2 = new Patient { Name = "Kavindu Silva", NIC = "198812304958", Gender = "Male", PhoneNumber = "+94719876543", DateOfBirth = new DateTime(1988, 11, 20), BloodGroup = "O+", CreatedAt = seedDate, UpdatedAt = seedDate };
            var p3 = new Patient { Name = "Nimali Fernando", NIC = "199584739201", Gender = "Female", PhoneNumber = "+94754567890", DateOfBirth = new DateTime(1995, 3, 8), BloodGroup = "B+", CreatedAt = seedDate, UpdatedAt = seedDate };
            var p4 = new Patient { Name = "Sunil Jayasinghe", NIC = "197530491029", Gender = "Male", PhoneNumber = "+94701122334", DateOfBirth = new DateTime(1975, 8, 30), BloodGroup = "AB+", CreatedAt = seedDate, UpdatedAt = seedDate };

            context.Patients.AddRange(p1, p2, p3, p4);
            await context.SaveChangesAsync();

            var firstSchedule = context.DoctorSchedules.FirstOrDefault();
            int scheduleId = firstSchedule?.Id ?? 1;

            var app1 = new Appointment { PatientId = p1.Id, DoctorId = doctor1.Id, ScheduleId = scheduleId, AppointmentDate = DateTime.UtcNow.Date.AddDays(1).AddHours(9), Status = AppointmentStatus.Confirmed, ReasonForVisit = "Acute chest pain & shortness of breath", CreatedAt = seedDate, UpdatedAt = seedDate };
            var app2 = new Appointment { PatientId = p2.Id, DoctorId = doctor2.Id, ScheduleId = scheduleId, AppointmentDate = DateTime.UtcNow.Date.AddDays(2).AddHours(10), Status = AppointmentStatus.Pending, ReasonForVisit = "Severe migraine with dizziness", CreatedAt = seedDate, UpdatedAt = seedDate };
            var app3 = new Appointment { PatientId = p3.Id, DoctorId = doctor3.Id, ScheduleId = scheduleId, AppointmentDate = DateTime.UtcNow.Date.AddDays(3).AddHours(11), Status = AppointmentStatus.Confirmed, ReasonForVisit = "Skin rash & itching", CreatedAt = seedDate, UpdatedAt = seedDate };
            var app4 = new Appointment { PatientId = p4.Id, DoctorId = doctor1.Id, ScheduleId = scheduleId, AppointmentDate = DateTime.UtcNow.Date.AddDays(4).AddHours(14), Status = AppointmentStatus.Pending, ReasonForVisit = "Right knee pain & swelling", CreatedAt = seedDate, UpdatedAt = seedDate };

            context.Appointments.AddRange(app1, app2, app3, app4);
            await context.SaveChangesAsync();

            var ta1 = new TriageAssessment { AppointmentId = app1.Id, RawSymptoms = "Acute severe chest pain radiating to left shoulder with shortness of breath", UrgencyScore = 95, UrgencyLevel = UrgencyLevel.Emergency, RecommendedSpecialty = "Cardiology", ReasoningTrace = "[Symptom Triage Agent] Parsed 2 symptom entries for raw symptoms: 'Acute severe chest pain radiating to left shoulder'. Highest severity score: 9/10. Emergency red flags: Detected. Matched specialty: Cardiology. Assessed urgency level: Emergency (Score: 95/100).", CreatedAt = seedDate, UpdatedAt = seedDate };
            var ta2 = new TriageAssessment { AppointmentId = app2.Id, RawSymptoms = "Severe migraine headache with sudden dizziness and light sensitivity", UrgencyScore = 82, UrgencyLevel = UrgencyLevel.Emergency, RecommendedSpecialty = "Neurology", ReasoningTrace = "[Symptom Triage Agent] Parsed 2 symptom entries. Highest severity rating: 8/10. Matched specialty: Neurology. Emergency red flags: Detected. Assessed urgency level: Emergency.", CreatedAt = seedDate.AddHours(1), UpdatedAt = seedDate.AddHours(1) };
            var ta3 = new TriageAssessment { AppointmentId = app3.Id, RawSymptoms = "Red itching skin rash on forearm after contacting new detergent", UrgencyScore = 40, UrgencyLevel = UrgencyLevel.Medium, RecommendedSpecialty = "Dermatology", ReasoningTrace = "[Symptom Triage Agent] Parsed 1 symptom entry. Highest severity rating: 4/10. Emergency red flags: None. Matched specialty: Dermatology. Assessed urgency level: Medium.", CreatedAt = seedDate.AddHours(2), UpdatedAt = seedDate.AddHours(2) };
            var ta4 = new TriageAssessment { AppointmentId = app4.Id, RawSymptoms = "Right knee pain and swelling following basketball game", UrgencyScore = 65, UrgencyLevel = UrgencyLevel.High, RecommendedSpecialty = "Orthopedics", ReasoningTrace = "[Symptom Triage Agent] Parsed 1 symptom entry. Highest severity rating: 6/10. Matched specialty: Orthopedics. Assessed urgency level: High.", CreatedAt = seedDate.AddHours(3), UpdatedAt = seedDate.AddHours(3) };

            context.TriageAssessments.AddRange(ta1, ta2, ta3, ta4);
            await context.SaveChangesAsync();

            context.SymptomLogs.AddRange(
                new SymptomLog { TriageId = ta1.Id, SymptomKeyword = "chest pain", SeverityRating = 9, DurationInDays = 1, CreatedAt = seedDate, UpdatedAt = seedDate },
                new SymptomLog { TriageId = ta1.Id, SymptomKeyword = "shortness of breath", SeverityRating = 8, DurationInDays = 1, CreatedAt = seedDate, UpdatedAt = seedDate },
                new SymptomLog { TriageId = ta2.Id, SymptomKeyword = "migraine", SeverityRating = 8, DurationInDays = 2, CreatedAt = seedDate, UpdatedAt = seedDate },
                new SymptomLog { TriageId = ta2.Id, SymptomKeyword = "dizziness", SeverityRating = 7, DurationInDays = 1, CreatedAt = seedDate, UpdatedAt = seedDate },
                new SymptomLog { TriageId = ta3.Id, SymptomKeyword = "skin rash", SeverityRating = 4, DurationInDays = 3, CreatedAt = seedDate, UpdatedAt = seedDate },
                new SymptomLog { TriageId = ta4.Id, SymptomKeyword = "knee pain", SeverityRating = 6, DurationInDays = 2, CreatedAt = seedDate, UpdatedAt = seedDate }
            );

            context.Referrals.AddRange(
                new Referral { TriageId = ta1.Id, TargetSpecialty = "Cardiology", Status = ReferralStatus.Generated, CreatedAt = seedDate, UpdatedAt = seedDate },
                new Referral { TriageId = ta2.Id, TargetSpecialty = "Neurology", Status = ReferralStatus.Reviewed, CreatedAt = seedDate.AddHours(1), UpdatedAt = seedDate.AddHours(1) },
                new Referral { TriageId = ta3.Id, TargetSpecialty = "Dermatology", Status = ReferralStatus.Assigned, CreatedAt = seedDate.AddHours(2), UpdatedAt = seedDate.AddHours(2) },
                new Referral { TriageId = ta4.Id, TargetSpecialty = "Orthopedics", Status = ReferralStatus.Generated, CreatedAt = seedDate.AddHours(3), UpdatedAt = seedDate.AddHours(3) }
            );

            await context.SaveChangesAsync();
        }
    }
}
