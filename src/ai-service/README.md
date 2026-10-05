# ChannelCenter Safety Auditor

This is an internal FastAPI service for the Phase 2 Safety Auditor. It is a
deterministic path and does not require a paid model or an external model
provider. LangGraph still provides the bounded `Plan -> Validate -> RiskAssess
-> PauseOrComplete` execution graph and exposes the distinct Planner,
EvidenceValidator, RiskClassifier and SafetyAuditor roles.

## Local startup

```powershell
cd src\ai-service
py -3.12 -m venv .venv
.\.venv\Scripts\python -m pip install -r requirements-dev.txt
Copy-Item .env.example .env
# Set INTERNAL_SERVICE_KEY to the same local-only value as ASP.NET SafetyAuditor:InternalServiceKey.
.\.venv\Scripts\python -m uvicorn app.main:app --host 127.0.0.1 --port 8001
```

`GET /health/live` is a liveness check. `GET /health/ready` requires the
internal shared secret to be configured. Audit requests and pause calls require
`X-Internal-Service-Key`; browser clients must never call this service.

## Deployment boundary

Build the included Dockerfile as a separate private container/service. Put it
behind the ASP.NET Core API or a private network policy. React and Flutter
continue to call ASP.NET Core with their normal JWTs. Only ASP.NET Core calls
`POST /internal/v1/safety-audits/{workflowId}` and only the auditor calls the
internal pause endpoint with the shared secret. Do not expose this service,
Swagger, or its port directly to browsers.

The service rejects prompt-injection phrases, unknown tool names, oversized
payloads and malformed tool results. Tool URLs and methods are fixed in the
allow-list, with bounded timeout and retry settings. It never executes the
proposed action; it only returns a structured recommendation.
