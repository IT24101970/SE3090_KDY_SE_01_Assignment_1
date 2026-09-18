# Component 1: Patient Management & Appointment Lifecycle
**Users:** Id (PK), FullName, Email, PasswordHash, Role (Patient, Doctor, Admin), CreatedAt, UpdatedAt.

**Patients:** Id (PK), UserId (FK), Name, EmergencyContact, DateOfBirth, Gender, PhoneNumber, Email, NIC, CreatedAt, UpdatedAt.

**Appointments:** Id (PK), PatientId (FK), DoctorId (FK), ScheduleId (FK), AppointmentDate, Status (Pending, Confirmed, Cancelled), ReasonForVisit, CancelReason, CreatedAt, UpdatedAt.

**IntakeAgentLogs:** Id (PK), PatientId (FK), ToolCalled, InputPayload (JSONB), OutputPayload (JSONB), SessionNotes, CreatedAt, UpdatedAt.


# Component 2: Doctor Scheduling & Consultation
**Doctors**: Id (PK), UserId (FK), SpecialtyId (FK), Qualifications, CreatedAt, UpdatedAt.

**DoctorSchedules**: Id (PK), DoctorId (FK), RoomId (FK), StartTime, EndTime, MaxPatients, CreatedAt, UpdatedAt.

**Consultations**: Id (PK), AppointmentId (FK), ClinicalNotes, PrescriptionData (JSONB), AttendanceStatus (Pending, Present, NoShow), CreatedAt, UpdatedAt.

**ConsultationRooms**: Id (PK), RoomName, Floor, IsActive, CreatedAt, UpdatedAt.

**DoctorLeaves**: Id (PK), DoctorId (FK), StartDate, EndDate, Reason, Status (Pending, Approved, Rejected), CreatedAt, UpdatedAt.


# Component 3: Medical Triage & Specialist Matching
**Specialties**: Id (PK), Name, Description, CreatedAt, UpdatedAt.

**PreConsultationQuestionnaires**: Id (PK), PatientId (FK), ResponsesData (JSONB), CreatedAt, UpdatedAt.

**TriageAssessments**: Id (PK), PatientId (FK), QuestionnaireId (FK), RawSymptoms (Text), UrgencyScore (Integer), UrgencyLevel (Low, Medium, High, Emergency), ReasoningTrace (Text), RecommendedSpecialtyId (FK), CreatedAt, UpdatedAt.

**SymptomLogs**: Id (PK), TriageId (FK), SymptomKeyword, SeverityRating, DurationInDays, CreatedAt, UpdatedAt.

**Referrals**: Id (PK), TriageId (FK), TargetSpecialtyId (FK), Status (Generated, Reviewed, Assigned), CreatedAt, UpdatedAt.


# Component 4: Admin Oversight & AI Safety
**AgentWorkflows**: Id (PK), Objective, Status (Running, PausedForApproval, Completed), RequiresHumanApproval (Boolean), CreatedAt, UpdatedAt.

**AuditLogs**: Id (PK), WorkflowId (FK), AgentName, ToolCalled, ToolOutput (JSONB), CreatedAt, UpdatedAt.

**AdminApprovals**: Id (PK), WorkflowId (FK), AdminUserId (FK), Decision (Approved, Rejected, Revised), CreatedAt, UpdatedAt.