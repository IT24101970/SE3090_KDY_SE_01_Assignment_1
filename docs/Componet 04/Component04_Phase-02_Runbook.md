# Safety Auditor Phase 2 runbook

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

## Deployment boundary

Deploy the FastAPI image as a private service/container on the server network.
Route only ASP.NET Core to it and restrict the Python container's egress to
the ASP.NET internal endpoint. Do not add the Python URL to CORS, frontend
configuration, browser code or public ingress. Rotate the internal shared
secret using the deployment secret manager.
