**Users:** Id (PK), Role (Admin, Doctor, Patient), Email, PasswordHash, CreatedAt, UpdatedAt.

**Patients:** Id (PK), UserId (FK), DateOfBirth, EmergencyContact, CreatedAt, UpdatedAt. 

**Appointments:** Id (PK), PatientId (FK), DoctorScheduleId (FK), TriageId (FK), Status (Pending, Confirmed, Cancelled), CreatedAt, UpdatedAt. 



**Doctors**: Id (PK), UserId (FK), SpecialtyId (FK), Qualifications, CreatedAt, UpdatedAt.

**DoctorSchedules**: Id (PK), DoctorId (FK), RoomNumber, StartTime, EndTime, MaxPatients, CreatedAt, UpdatedAt.

**Consultations**: Id (PK), AppointmentId (FK), ClinicalNotes, PrescriptionData (JSONB), CreatedAt, UpdatedAt.



**Specialties**: Id (PK), Name (e.g., Cardiology, Dermatology), Description, CreatedAt, UpdatedAt.

**TriageAssessments**: Id (PK), PatientId (FK), RawSymptoms (Text), UrgencyScore (Integer), RecommendedSpecialtyId (FK), CreatedAt, UpdatedAt.

**SymptomLogs**: Id (PK), TriageId (FK), SymptomKeyword, DurationInDays, CreatedAt, UpdatedAt.



**AgentWorkflows**: Id (PK), Objective, Status (Running, PausedForApproval, Completed), RequiresHumanApproval (Boolean), CreatedAt, UpdatedAt.

**AuditLogs**: Id (PK), WorkflowId (FK), AgentName, ToolCalled, ToolOutput (JSONB), CreatedAt, UpdatedAt. 

**AdminApprovals**: Id (PK), WorkflowId (FK), AdminUserId (FK), Decision (Approved, Rejected, Revised), CreatedAt, UpdatedAt 

