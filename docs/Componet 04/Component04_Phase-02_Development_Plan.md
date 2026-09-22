# Component 4 - Phase 2 Development Plan

## Agentic AI Integration: Safety Auditor Agent

**Owner:** Kavindu  
**Baseline:** Component 4 Phase 1  
**Primary outcome:** Integrate a Python/LangGraph Safety Auditor behind the ASP.NET Core API so that workflow proposals are validated, unsafe or high-impact work is paused for authorized human approval, and every execution ends in an auditable result or safe failure.

## 1. Purpose and scope

Phase 1 established the administrative workflow, approval state machine, audit-log API, analytics endpoints, JWT protection and test foundation. Phase 2 implements the actual agentic subsystem described in the assignment:

1. Receive a domain workflow objective and structured proposal from the ASP.NET Core backend.
2. Create a multi-step audit plan with distinct, visible agent responsibilities.
3. Call only allow-listed backend tools using validated JSON inputs.
4. Apply deterministic schema and business-rule checks before accepting a proposal.
5. Call `POST /api/admin/workflows/{id}/pause` when emergency urgency, a validation violation or another configured high-impact condition is detected.
6. Persist workflow progress, tool results, validation results, errors, approval requirement and final outcome.
7. Return a structured result to ASP.NET Core; React and Flutter continue to communicate only with ASP.NET Core.

This phase does **not** build the React approval dashboard or Flutter emergency screen. Those are Phase 3 consumers of the stable workflow and audit contracts.

## 2. Assignment alignment

| Assignment requirement | Phase 2 implementation evidence |
|---|---|
| Meaningful multi-step agentic problem | Audit an appointment/triage/scheduling proposal before it can cause a high-impact action |
| Structured plan and delegation | LangGraph state graph with `Plan`, `Validate`, `RiskAssess`, `PauseOrComplete` nodes |
| Distinct agents | `SafetyAuditor` validates policy and safety; `EvidenceValidator` checks required evidence/schema; `RiskClassifier` classifies urgency and impact |
| Controlled tools | Explicit tool registry with input models, output models, role restrictions and timeouts |
| Shared durable state | Existing `AgentWorkflow`, `AuditLog` and `AdminApproval` records extended or supplemented with plan, step, validation and result fields |
| Deterministic validation | JSON schema validation, required-field checks, urgency rules, workflow-status rules and allow-list checks outside the LLM |
| Human approval | Emergency or failed validation transitions the workflow to `PausedForApproval`; only an authorized admin can approve, reject or request revision |
| Observability and safe failure | Structured audit entries for each node/tool, timing, retry, error, validation decision and terminal outcome |
| Security | Internal service authentication, secret-free prompts, input limits, timeout/retry limits, output validation and no hidden reasoning persistence |

The group still needs the other three distinct business agents owned by Components 1-3. This component contributes the validation/safety role and must not be presented as a copy of those agents.

## 3. Target architecture and execution flow

```text
Flutter/React
    |
    | HTTPS REST (JWT, role-protected)
    v
ASP.NET Core API
    | create/load workflow and persist state
    | internal authenticated call
    v
Python Safety Auditor (LangGraph)
    | allow-listed read/validation tools
    | structured result callback
    v
ASP.NET Core API -> PostgreSQL
    | status, plan, results, audit logs
    v
Admin approval endpoints -> React dashboard (Phase 3)
```

The Python service must not be called directly by either client. ASP.NET Core remains the authoritative layer for identity, permissions, business rules, persistence and approval decisions.

### Recommended execution sequence

1. ASP.NET Core creates or loads a `Running` workflow.
2. ASP.NET Core sends a minimal signed request containing `workflowId`, objective, proposal/context, correlation ID and contract version to the internal Python endpoint.
3. LangGraph creates a bounded plan and records the plan summary.
4. `EvidenceValidator` checks the proposal shape and required evidence.
5. `RiskClassifier` calculates a deterministic risk category from structured fields; an LLM may assist with classification but cannot bypass deterministic rules.
6. `SafetyAuditor` evaluates policy violations and high-impact actions.
7. If emergency/high-impact/invalid, the service calls the internal pause operation and returns `PausedForApproval`.
8. If safe, it returns a validated result and ASP.NET Core records `Completed` (or the next business action state).
9. On timeout, exhausted retries or malformed output, the service records a safe failure and does not execute the proposed action.

## 4. Phase 2 workstreams

### Workstream A - Contract and state design

- Define versioned request/response models shared through documented JSON examples.
- Define `SafetyAuditRequest` fields: `workflowId`, `objective`, `proposal`, `sourceAgent`, `correlationId`, `contractVersion`.
- Define `SafetyAuditResponse` fields: `workflowId`, `status`, `riskLevel`, `requiresApproval`, `violations`, `validatedOutput`, `steps`, `toolCalls`, `error`, `completedAt`.
- Define allowed statuses and transitions: `Running -> Completed`, `Running -> PausedForApproval`, `Running -> SafeFailed`; approval actions remain in ASP.NET Core.
- Decide whether to extend `AgentWorkflow` with JSONB/text fields for `Plan`, `ValidationSummary`, `FinalOutcome`, `CorrelationId` and `ContractVersion`, or add normalized workflow-step/result tables. Prefer normalized rows for queryable audit data and retain only bounded summaries.
- Add indexes for workflow status, creation time and correlation ID.

**Deliverable:** API contract, state-transition diagram, migration and ADR entry.

### Workstream B - Python service foundation

- Create an isolated service under a clearly named project directory, for example `src/agents/safety-auditor/`.
- Pin Python and package versions; add `.env.example` with names only, never real secrets.
- Add configuration for the ASP.NET Core base URL, internal service credential, model/provider, timeout, retry count and maximum input/output sizes.
- Implement health/readiness endpoints for local orchestration and deployment checks.
- Use async HTTP with connection timeouts and cancellation.
- Add correlation IDs to every request, log entry and callback.

**Deliverable:** Reproducible local startup and a health check that does not expose secrets.

### Workstream C - LangGraph graph and distinct roles

- Implement a typed graph state containing workflow metadata, plan, current step, validated proposal, violations, tool results, approval decision and terminal outcome.
- Implement a bounded planner that emits a fixed schema and a maximum number of steps; reject plans containing unknown roles or tools.
- Implement `EvidenceValidator` with deterministic checks first.
- Implement `RiskClassifier` with explicit categories such as `Low`, `Medium`, `High`, `Emergency`.
- Implement `SafetyAuditor` to evaluate policy rules and produce a structured recommendation, not an executable command.
- Implement terminal routing:
  - `PauseForApproval` for emergency, high-impact or validation violation.
  - `Complete` only when all required checks pass.
  - `SafeFail` for malformed responses, unavailable tools, timeouts or exhausted retries.
- Do not persist chain-of-thought or hidden reasoning; persist concise reasons, rule IDs, evidence references and decisions only.

**Deliverable:** Working graph with visible nodes and deterministic routing.

### Workstream D - Controlled tools and ASP.NET integration

- Build a registry containing only the tools required by this audit, for example:
  - `GetWorkflowContext(workflowId)`
  - `GetTriageSummary(workflowId)`
  - `GetScheduleProposal(workflowId)`
  - `ValidateBusinessRules(proposal)`
  - `PauseWorkflow(workflowId, reason, violation)`
  - `SubmitAuditLog(...)`
- Give each tool a Pydantic input/output model and reject extra or missing fields.
- Enforce per-tool timeout, maximum response size, retry limit and allowed HTTP method/path.
- Do not allow the model to construct arbitrary URLs, SQL, filesystem paths, shell commands or admin actions.
- Add a dedicated internal authentication mechanism for Python-to-ASP.NET calls. Do not reuse a browser admin token or bypass `[Authorize(Roles = "Admin")]`. The API must distinguish internal service authentication from end-user authorization and still authorize workflow ownership/action at the server.
- Add an ASP.NET orchestration service, such as `ISafetyAuditorService`, to start the agent, persist `Running`, handle callbacks/results and translate failures into safe workflow states.
- Add idempotency using `workflowId` plus `correlationId`; repeated delivery must not create duplicate pause actions or audit records.

**Deliverable:** End-to-end API-to-agent-to-API call with no direct client-to-Python traffic.

### Workstream E - Persistence, audit and operational visibility

- Record plan creation, each agent step, each tool call, validation result, pause request, error/retry and terminal outcome.
- Store timestamps, duration, agent name, tool name, input/output summaries and validation rule IDs; redact tokens, passwords, unnecessary patient data and hidden reasoning.
- Ensure audit writes are linked to the workflow and correlation ID.
- Update AI metrics to report completed, paused, safe-failed, validation-failed, retry and average-latency counts.
- Add structured logs suitable for local debugging and CI artifacts.

**Deliverable:** A workflow detail response can explain what happened without reading Python logs.

### Workstream F - Tests and evaluation evidence

- Python unit tests for each validator, risk rule, graph route, tool schema, timeout and retry policy.
- ASP.NET unit tests for orchestration success, pause, safe failure, duplicate delivery, malformed response and authorization.
- Contract tests for request/response JSON and status transitions.
- Integration test using a fake Python service; do not require a paid model or network service in CI.
- Golden cases covering:
  - safe proposal -> validated completion;
  - emergency urgency -> paused approval;
  - missing/invalid evidence -> paused or safe failure;
  - prompt injection in proposal -> ignored/rejected;
  - tool timeout/unavailable service -> safe failure;
  - unauthorized approval/internal call -> 401/403;
  - duplicate callback -> idempotent result.
- Capture evidence for planning, delegation, tools, persisted state, deterministic validation, approval enforcement and auditable completion/safe failure.

**Deliverable:** Automated test report plus a repeatable evaluation script with sanitized fixtures.

## 5. Proposed implementation order

| Iteration | Tasks | Exit criteria |
|---|---|---|
| 1. Design and contracts | State diagram, JSON schemas, internal auth decision, migration design, ADR update | Contracts reviewed and examples agree with Phase 1 DTO/status names |
| 2. Service skeleton | Python project, configuration, health endpoint, typed models, fake model/tool adapters | Service starts locally and health/readiness checks pass |
| 3. Deterministic core | Validators, risk rules, tool registry, bounded graph state and terminal routes | Golden fixtures produce repeatable decisions without an LLM |
| 4. Backend integration | ASP.NET orchestration service, internal auth, pause callback, persistence and idempotency | One workflow completes; one emergency workflow pauses |
| 5. Hardening | Redaction, limits, retries, prompt-injection handling, failure paths and metrics | No unsafe action executes on malformed, unauthorized or unavailable input |
| 6. Evidence and handoff | CI tests, API examples, sequence diagram, runbook and Phase 3 integration notes | Phase 3 can consume stable polling/detail/approval contracts |

## 6. Definition of done

- [ ] A Flutter- or API-originated workflow reaches the Python Safety Auditor only through ASP.NET Core.
- [ ] The graph shows a structured plan, distinct safety responsibilities and bounded delegation.
- [ ] All tools are allow-listed, typed, least-privilege and protected by timeout/retry limits.
- [ ] Safe, emergency, validation-failure, prompt-injection and unavailable-service cases have deterministic outcomes.
- [ ] Emergency or high-impact proposals call the Phase 1 pause endpoint and become `PausedForApproval`.
- [ ] Only an authorized admin can approve, reject or revise; approval decisions are persisted.
- [ ] Workflow state and execution summaries are durable and visible through existing admin endpoints.
- [ ] No passwords, tokens, hidden reasoning or unnecessary sensitive data are persisted or logged.
- [ ] At least one workflow produces a validated auditable result and at least one produces a recorded safe failure.
- [ ] Unit, contract, integration and agent-evaluation tests run in CI without a paid external model.
- [ ] README/runbook, architecture diagram, ADR and sanitized demo fixtures are updated.

## 7. Risks and mitigations

| Risk | Mitigation |
|---|---|
| Python service bypasses the mandatory backend rule | Keep the internal URL private; expose agent start/status only through ASP.NET Core |
| Existing admin-only pause endpoint rejects the agent | Add explicit internal service authentication and a server-side internal orchestration path; never embed a human JWT |
| LLM returns unsafe or unstructured output | Pydantic/schema validation and deterministic business rules decide whether execution is allowed |
| Duplicate callbacks pause or log a workflow repeatedly | Correlation ID, idempotency check and transactional status transition |
| Model/provider unavailable during demo or CI | Deterministic fake adapter and golden fixtures; local model/provider is optional for the assessed path |
| Sensitive healthcare data leaks to logs/model | Minimize context, redact fields, use synthetic fixtures and enforce log/prompt size limits |
| Phase 3 depends on unstable response shapes | Freeze versioned DTOs and provide Swagger examples before dashboard work begins |

## 8. Phase 3 handoff contract

Phase 3 may begin when the following interfaces are stable:

- `GET /api/admin/workflows?status=PausedForApproval`
- `GET /api/admin/workflows/{id}` including bounded audit and validation summaries
- Existing approve/reject/revise workflow endpoint and response
- Emergency alert payload containing workflow ID, risk level, reason, timestamp and required action
- Analytics metrics for completed, paused, failed and validation outcomes

The dashboard must never infer safety decisions from free-form log text; it should render the structured fields produced in this phase.
