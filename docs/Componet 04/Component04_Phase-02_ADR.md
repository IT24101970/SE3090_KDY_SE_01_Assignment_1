# ADR: Private deterministic Safety Auditor boundary

## Status

Accepted for Component 04 Phase 2.

## Decision

The Safety Auditor runs as a separate FastAPI container behind the ASP.NET
Core API. ASP.NET Core remains the authority for identity, persistence,
workflow transitions and approval decisions. The auditor receives only a
versioned, bounded proposal and returns a typed result. React and Flutter
never call the Python service directly.

The graph is deliberately bounded and deterministic:

`Plan -> Validate -> RiskAssess -> PauseOrComplete`

The four visible roles are Planner, EvidenceValidator, RiskClassifier and
SafetyAuditor. Only typed tools in the Python registry can make an HTTP call.
The only mutating tool is the internal pause operation, protected by
`X-Internal-Service-Key`. No model, shell, SQL, filesystem or arbitrary URL
tool is enabled. A paid model is not required.

## State and failure policy

ASP.NET Core stores the correlation ID, contract version, bounded plan,
validation summary, risk, final outcome and error fields on `AgentWorkflow`.
`Running` may become `Completed`, `PausedForApproval` or `SafeFailed`.
`workflowId + correlationId` makes callbacks and pause requests idempotent;
duplicate deliveries do not create another pause audit record.

Prompt-injection content, invalid evidence, high-impact actions and emergency
urgency cannot complete. Timeout, unavailable service, oversized or malformed
responses become `SafeFailed`, and no proposed action is executed.

## Migration

`dotnet-ef` is available in the development environment. Generate and apply
the migration from `src\backend\ChannelCenter.API`:

```powershell
dotnet ef migrations add SafetyAuditorWorkflowFields `
  --project .\ChannelCenter.API.csproj `
  --startup-project .\ChannelCenter.API.csproj `
  --output-dir Migrations
dotnet ef database update --project .\ChannelCenter.API.csproj
```

The migration must be reviewed before applying to production. Configure the
shared key through a secret store or environment variable; never commit it.
