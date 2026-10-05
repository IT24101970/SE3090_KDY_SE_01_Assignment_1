The Safety Auditor now runs immediately after Component 2 successfully saves an appointment. It retrieves the persisted appointment context through ASP.NET Core, including:
•
Appointments
◦
Doctor ID
◦
Schedule ID
◦
Appointment date
◦
Appointment status
•
TriageAssessments
◦
Urgency level
◦
Recommended specialty
◦
Raw symptoms
•
Doctors
◦
Assigned doctor
◦
Doctor specialty
•
DoctorSchedules
◦
Schedule date/time
◦
Doctor ownership
◦
Capacity and booked patients
◦
Availability
•
DoctorLeaves
◦
Approved leave conflicts
The Python service still does not access PostgreSQL directly. It calls a protected ASP.NET Core read-only context endpoint.
The auditor now evaluates:
•
High/Emergency patients must be assigned to the recommended specialty.
•
Emergency appointments must be scheduled on the same day.
•
High urgency appointments must be within 24 hours.
•
Medium urgency appointments must be within 3 days.
•
Low and Medium urgency patients may use clinically related specialists.
•
Missing doctors, unavailable schedules, capacity conflicts, leave conflicts, past dates, and appointments outside the selected schedule are flagged.
•
Low-risk cases do not require an exact specialty match, but clearly unrelated assignments such as common respiratory symptoms assigned to Dentistry are rejected.
When a serious mismatch is detected:
1.
The appointment remains saved.
2.
A linked AgentWorkflow is created.
3.
The workflow changes to PausedForApproval.
4.
Audit details are stored in AuditLogs.
5.
An administrator can review and approve or reject it.
Administrators can also restart any paused workflow through:
POST /api/admin/workflows/{id}/restart
Restarting generates a new correlation ID and reruns the Safety Auditor against the latest appointment and triage data.