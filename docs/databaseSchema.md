# Component 1: Patient Management & Appointment Lifecycle
**Users:** Id (PK), FullName, Email, PasswordHash, Role (Patient, Doctor, Admin), CreatedAt, UpdatedAt.

**Patients:** Id (PK), UserId (FK), Name, EmergencyContact, DateOfBirth, Gender, PhoneNumber, Email, NIC, CreatedAt, UpdatedAt.

**Appointments:** Id (PK), PatientId (FK), DoctorId (FK), ScheduleId (FK), AppointmentDate, Status (Pending, Confirmed, Cancelled), ReasonForVisit, CancelReason, CreatedAt, UpdatedAt.

# Component 2: Doctor Scheduling & Consultation
**Doctors**: Id (PK), UserId (FK), Specialty, Qualifications, CreatedAt, UpdatedAt.

**DoctorSchedules**: Id (PK), DoctorId (FK), RoomId (FK), StartTime, EndTime, MaxPatients, CreatedAt, UpdatedAt.

**ConsultationRooms**: Id (PK), RoomName, Floor, IsActive, CreatedAt, UpdatedAt.

**DoctorLeaves**: Id (PK), DoctorId (FK), StartDate, EndDate, Reason, Status (Pending, Approved, Rejected), CreatedAt, UpdatedAt.


# Component 3: Medical Triage & Specialist Matching
**TriageAssessments**: Id (PK), PatientId (FK), RawSymptoms (Text), UrgencyScore (Integer), UrgencyLevel (Low, Medium, High, Emergency), ReasoningTrace (Text), RecommendedSpecialtyId, CreatedAt, UpdatedAt.

# Component 4: Admin Oversight & AI Safety
**AgentWorkflows**: Id (PK), Objective, Status (Running, PausedForApproval, Completed), RequiresHumanApproval (Boolean), CreatedAt, UpdatedAt.

**AuditLogs**: Id (PK), WorkflowId (FK), AgentName, ToolCalled, ToolOutput (JSONB), CreatedAt, UpdatedAt.