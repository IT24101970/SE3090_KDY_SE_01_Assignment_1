# Safety Auditor Phase 2 runbook

## Component 2 appointment flow

`POST /api/appointments` is the Component 2 final-save boundary. The API first
commits the appointment with its existing `doctorId`, `scheduleId`,
`appointmentDate` and `Pending` status. It then synchronously creates (or
reuses) the linked `AgentWorkflow` and calls the private Safety Auditor before
returning the successful appointment response. No additional appointment
status is introduced and an auditor outage never rolls back the saved
appointment; the linked workflow is marked `SafeFailed` for an administrator.

The Python service has no PostgreSQL access. Its allow-listed
`GetAppointmentContext` tool calls the shared-secret ASP.NET endpoint
`GET /api/internal/v1/safety-auditor/appointments/{appointmentId}/context`.
That read-only endpoint queries the persisted `Appointment`,
`TriageAssessment`, `Doctor`/`Specialty`, and `DoctorSchedule`, including
current active booking capacity. The resulting typed context is evaluated
deterministically:

- Emergency: appointment must be on the same day.
- High: appointment must be within 24 hours.
- Medium: appointment must be within three days and use a clinically related
  specialty.
- Low: any future available slot is allowed, except clearly unrelated
  assignments such as common cold/cough to Dentistry.
- Missing doctor/schedule, unavailable schedule, hard specialty mismatches and
  urgency-window violations pause the linked workflow while leaving the
  appointment saved.

Replaying the same appointment audit correlation is idempotent. The internal
pause endpoint only updates the workflow and writes its audit log; it never
starts another audit, preventing recursion.

## Local configuration

Set the same value in both services, using a local secret only:

```powershell
$env:SafetyAuditor__InternalServiceKey = "local-development-secret"
$env:INTERNAL_SERVICE_KEY = "local-development-secret"
```

Start the Python service separately:

```powershell
cd src\ai-service
py -3.12 -m venv .venv
.\.venv\Scripts\python -m pip install -r requirements.txt
.\.venv\Scripts\python -m uvicorn app.main:app --host 127.0.0.1 --port 8001
```

Check `GET http://127.0.0.1:8001/health/live` and
`GET http://127.0.0.1:8001/health/ready`. Readiness is intentionally false
when the shared key is missing.

## Workflow execution

An authorized admin starts an audit through
`POST /api/admin/workflows/{id}/safety-audit`. ASP.NET Core marks the workflow
`Running`, sends a versioned request to the private Python service, and stores
the result. Emergency, high-impact and validation-failed proposals invoke the
internal pause route and become `PausedForApproval`. Service timeout,
malformed response and exhausted retries become `SafeFailed`.

Use `GET /api/admin/workflows/{id}` to inspect bounded plan, validation,
risk, outcome, error and audit fields. Human approval remains on the existing
JWT-protected admin endpoint. Replaying the same correlation ID is safe and
does not repeat a pause action.

An administrator can restart **any** paused workflow with
`POST /api/admin/workflows/{id}/restart` and an optional `{ "reason": "..." }`.
Appointment-linked workflows receive a new correlation ID and are audited
again; non-appointment workflows are simply returned to `Running`. This
endpoint is independent of the particular violation that caused the pause.

## Deployment boundary

Deploy the FastAPI image as a private service/container on the server network.
Route only ASP.NET Core to it and restrict the Python container's egress to
the ASP.NET internal endpoint. Do not add the Python URL to CORS, frontend
configuration, browser code or public ingress. Rotate the internal shared
secret using the deployment secret manager.
