**Users:** Id (PK), Role (Admin, Doctor, Patient), Email, PasswordHash, CreatedAt, UpdatedAt[cite: 4].

**Patients:** Id (PK), UserId (FK), DateOfBirth, EmergencyContact, CreatedAt, UpdatedAt[cite: 4]. 

**Appointments:** Id (PK), PatientId (FK), DoctorScheduleId (FK), TriageId (FK), Status (Pending, Confirmed, Cancelled), CreatedAt, UpdatedAt[cite: 4]. 



**Doctors**: Id (PK), UserId (FK), SpecialtyId (FK), Qualifications, CreatedAt, UpdatedAt[cite: 4].

**DoctorSchedules**: Id (PK), DoctorId (FK), RoomNumber, StartTime, EndTime, MaxPatients, CreatedAt, UpdatedAt[cite: 4].

**Consultations**: Id (PK), AppointmentId (FK), ClinicalNotes, PrescriptionData (JSONB), CreatedAt, UpdatedAt[cite: 4].



**Specialties**: Id (PK), Name (e.g., Cardiology, Dermatology), Description, CreatedAt, UpdatedAt[cite: 2, 4].

**PreConsultationQuestionnaires**: Id (PK), PatientId (FK), ResponsesData (JSONB), CreatedAt, UpdatedAt[cite: 2, 3].

**TriageAssessments**: Id (PK), PatientId (FK), QuestionnaireId (FK), RawSymptoms (Text), UrgencyScore (Integer), UrgencyLevel (Low, Medium, High, Emergency), ReasoningTrace (Text), RecommendedSpecialtyId (FK), CreatedAt, UpdatedAt[cite: 2, 3, 4].

**SymptomLogs**: Id (PK), TriageId (FK), SymptomKeyword, SeverityRating, DurationInDays, CreatedAt, UpdatedAt[cite: 2, 3, 4].

**Referrals**: Id (PK), TriageId (FK), TargetSpecialtyId (FK), Status (Generated, Reviewed, Assigned), CreatedAt, UpdatedAt[cite: 2, 3].



**AgentWorkflows**: Id (PK), Objective, Status (Running, PausedForApproval, Completed), RequiresHumanApproval (Boolean), CreatedAt, UpdatedAt[cite: 4].

**AuditLogs**: Id (PK), WorkflowId (FK), AgentName, ToolCalled, ToolOutput (JSONB), CreatedAt, UpdatedAt[cite: 4]. 

**AdminApprovals**: Id (PK), WorkflowId (FK), AdminUserId (FK), Decision (Approved, Rejected, Revised), CreatedAt, UpdatedAt[cite: 4].