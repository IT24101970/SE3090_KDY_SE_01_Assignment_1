# Component 4 - Phase 3 Development Plan

## React AI Approval Dashboard and Flutter Emergency Alert View

**Owner:** Kavindu  
**Prerequisites:** Component 4 Phase 1 backend and Phase 2 Safety Auditor integration  
**Primary outcome:** Deliver the web and mobile user interfaces that allow authorized administrators to monitor Safety Auditor workflows, review structured evidence, approve/reject/revise paused workflows, and notify operational users of emergency outcomes through Flutter.

## 1. Purpose and scope

Phase 3 is the presentation and cross-platform integration phase for Component 4. It consumes the stable ASP.NET Core workflow contracts produced by Phases 1 and 2:

- **React web application:** administrative monitoring, audit review, workflow details, analytics and approval actions.
- **Flutter mobile application:** emergency alerts, workflow status and acknowledgement for operational users.
- **ASP.NET Core API:** the only backend used by either client for authentication, authorization, business rules, workflow state and audit data.

The Python Safety Auditor is not called directly by React or Flutter. The clients must not implement approval rules locally, infer risk from free-form logs, or mutate workflow state outside the authorized ASP.NET Core endpoints.

## 2. Assignment alignment

| Assignment requirement | Phase 3 implementation evidence |
|---|---|
| React administrative/staff application | Protected admin dashboard for workflow monitoring, approval and reporting |
| Flutter operational application | Emergency alert and workflow-status screens with notification/deep-link handling |
| Shared backend rule | Both clients use the same ASP.NET Core REST API, PostgreSQL data and authorization model |
| Agentic workflow visibility | Plan summary, agent steps, tool calls, validation results, risk level, pause reason and terminal outcome |
| Human approval | Admin-only approve, reject and revise actions with confirmation and audit feedback |
| Search/filter/pagination | React workflow queue supports status, risk, date, agent, workflow ID and text filters |
| Loading/error/empty states | Explicit states for API loading, unauthorized access, empty queues, network failures and stale data |
| Secure client integration | JWT handling, protected routes/screens, role checks, HTTPS configuration and no secrets in source |
| End-to-end evidence | Flutter initiates or displays a shared workflow; React approves it; Flutter receives the updated status |
| Testing | React component/API/protected-route tests, Flutter widget/provider/API tests and one cross-platform scenario |

## 3. Target user journeys

### Journey A - Admin reviews an emergency workflow

1. Admin signs in to React.
2. Dashboard requests `GET /api/admin/workflows?status=PausedForApproval`.
3. Queue displays workflow objective, risk level, pause reason, created time and age.
4. Admin opens a workflow detail page.
5. React loads `GET /api/admin/workflows/{id}` and renders structured plan, agent steps, validation findings, tool summaries and audit history.
6. Admin chooses **Approve**, **Reject** or **Request revision**.
7. React sends the existing approval request to ASP.NET Core.
8. API enforces the admin role, validates the transition, persists `AdminApproval` and returns the updated status.
9. React refreshes the detail and queue; the decision is visible in the audit history.

### Journey B - Mobile user receives an emergency outcome

1. A Safety Auditor workflow is paused or completed with an emergency/high-risk result.
2. Flutter obtains workflow status through the ASP.NET Core API after login or refresh.
3. An emergency alert card/screen shows only the structured risk, reason, required action, timestamp and workflow ID.
4. The user acknowledges or opens the workflow status; acknowledgement is recorded only through an approved API operation if that operation is defined by the backend.
5. After an admin decision, Flutter refreshes the shared workflow and displays the new status.

The exact notification mechanism may be implemented as in-app polling first. Push notifications are optional unless the group selects a third-party notification service and documents the integration.

## 4. API contract required before UI implementation

Phase 3 should start only after these response shapes are confirmed in Swagger and represented by typed client models:

| Purpose | Endpoint/contract | Client |
|---|---|---|
| Authentication | Existing login endpoint returning JWT, user ID and role | React and Flutter |
| Workflow queue | `GET /api/admin/workflows?status=PausedForApproval` | React |
| Workflow detail | `GET /api/admin/workflows/{id}` with bounded audit/validation summaries | React and Flutter |
| Approval | Existing `POST /api/admin/workflows/{id}/approve` with `Approved`, `Rejected` or `Revised` | React |
| Audit search | `GET /api/admin/audit-logs` with filters and pagination | React |
| Analytics | `/api/admin/analytics/overview`, `ai-metrics` and related endpoints | React |
| Mobile status | Shared workflow/status endpoint authorized for the operational user role | Flutter |
| Emergency alert | Structured risk, reason, timestamp, required action and workflow ID | Flutter |

If the current admin-only workflow detail endpoint cannot support the intended Flutter role, add a narrowly scoped backend read endpoint rather than weakening admin authorization. Do not expose admin approval endpoints to mobile users.

## 5. Frontend architecture decisions

### React

- Use functional components, hooks and React Router.
- Use a small API client module for base URL, bearer token, JSON parsing, timeout and consistent error mapping.
- Use a centralized auth provider/context for the current user, token and role; protect admin routes with an `AdminRoute` component.
- Use a justified server-state approach. Prefer a query/cache library only if it is added deliberately; otherwise implement a typed workflow hook with cancellation, refresh and stale-state handling rather than scattering `fetch` calls across components.
- Keep UI state separate from server state: filters, selected tab and dialog state belong in components; workflow data belongs in the API/query layer.
- Use responsive CSS so the approval dashboard remains usable on laptop and tablet widths.

### Flutter

- Keep the existing app entry point and doctor-scheduling feature intact while adding a Component 4 feature module.
- Use the repository’s selected state-management approach consistently. If no project-wide decision exists, record and adopt Riverpod or Provider through an ADR rather than mixing patterns.
- Add typed API repositories and providers/controllers for authentication, workflow status and emergency alerts.
- Use secure token storage; never persist JWTs in plain preferences.
- Use named routes and reusable widgets for alert cards, status badges, timeline rows, loading and error states.
- Keep emergency presentation high visibility but do not make a client-side clinical or approval decision.

## 6. React implementation workstreams

### Workstream R1 - Application shell and authentication

- Confirm the Vite entry structure and create feature folders such as `src/features/admin-workflows`, `src/features/audit`, `src/features/analytics`, `src/auth` and `src/shared`.
- Add React Router routes for login, dashboard, workflow detail, audit logs and unauthorized/not-found pages.
- Implement login, logout, token expiry handling and role-based navigation.
- Add a development API base URL through environment configuration; keep `.env.example` names only.
- Add a global error boundary or equivalent top-level failure surface.

**Exit criteria:** Unauthenticated users cannot access admin routes; an authenticated Admin can reach the dashboard; expired/invalid tokens return to login without an infinite retry loop.

### Workstream R2 - Workflow queue and dashboard

- Build summary cards for running, paused, completed, safe-failed and emergency/high-risk workflows.
- Build a paginated queue with status, risk, date, agent and text filters.
- Add explicit refresh behavior, loading skeletons, empty state and retryable API error state.
- Poll only when needed and stop polling when the page is hidden/unmounted; avoid duplicate requests.
- Display timestamps in a consistent timezone and show workflow age for paused items.

**Exit criteria:** The dashboard can find a seeded paused workflow and clearly distinguish paused, completed and failed states.

### Workstream R3 - Workflow detail and evidence review

- Render the objective and structured final outcome.
- Render plan steps and distinct agent participation.
- Render validation rules, violation IDs, risk level and approval requirement.
- Render tool calls using safe summaries; do not display tokens, hidden reasoning or unnecessary patient data.
- Render audit entries chronologically with agent, tool, result summary, timestamp and duration.
- Show stale-data warning if the workflow changes while the page is open.

**Exit criteria:** An admin can explain why a workflow was paused without reading raw service logs.

### Workstream R4 - Approval controls

- Add a confirmation dialog for approve, reject and revise.
- Require a reason/comment for rejection or revision if the backend contract requires it.
- Disable actions while a request is pending and prevent double submission.
- Handle 401, 403, 404, conflict and validation responses with actionable messages.
- Refresh the workflow and audit history after a successful decision.
- Make approval controls keyboard accessible and clearly distinguish irreversible rejection from revision.

**Exit criteria:** Only the server response determines the resulting state; the UI cannot show a successful approval when the API rejects it.

### Workstream R5 - Audit and analytics views

- Add searchable/paginated audit log view using server-side filters.
- Add AI safety metrics cards/charts for paused, approved, rejected, revised, completed and safe-failed workflows.
- Include date range and agent filters where supported.
- Provide accessible labels, table headers and text alternatives for charts.

**Exit criteria:** An evaluator can trace a workflow from dashboard metrics to its detailed audit entries.

## 7. Flutter implementation workstreams

### Workstream F1 - Shared app foundation

- Add the Component 4 feature module without breaking existing doctor-scheduling screens.
- Add API base URL configuration for development and release builds.
- Implement secure authentication/token refresh behavior compatible with the ASP.NET Core API.
- Add route guards for authenticated operational users.
- Define typed models for workflow status, risk, alert and audit summary.

**Exit criteria:** A logged-in operational user can navigate to the emergency/workflow area and an unauthorized user cannot access protected data.

### Workstream F2 - Emergency alert view

- Build an alert list showing active emergency/high-risk workflows relevant to the current user.
- Build an alert detail view containing risk level, reason, required action, workflow ID, timestamp and current approval status.
- Use accessible color plus text/icon labels; do not communicate severity by color alone.
- Add clear loading, empty, offline and retry states.
- Prevent accidental destructive actions; Flutter is a status/alert surface, not the admin approval surface.

**Exit criteria:** A seeded emergency workflow is immediately understandable on a mobile screen and does not expose unrestricted admin data.

### Workstream F3 - Shared workflow status and refresh

- Add pull-to-refresh and lifecycle-aware refresh when returning to the app.
- Refresh after an admin decision so the mobile status changes from paused to running, completed or terminated.
- Show safe-failure and revision-required states distinctly.
- Add optional deep-link routing to a workflow detail screen if the selected notification approach supports it.

**Exit criteria:** The same workflow ID and status are visible consistently in React, Flutter, ASP.NET Core and PostgreSQL.

### Workstream F4 - Notification/device feature

- Decide whether the meaningful device feature is in-app notification, local notification, push notification, or date/time handling.
- If using notifications, register a device token only through ASP.NET Core and protect it as sensitive data.
- Handle permission denial, token refresh, duplicate notifications and offline delivery explicitly.
- If push infrastructure is not available, implement reliable in-app polling/local notification behavior and document the limitation.

**Exit criteria:** The selected device feature works in a reproducible local/demo setup and has a documented fallback.

## 8. Shared UI and security rules

- Use the ASP.NET Core API for all business data and decisions; no direct PostgreSQL, Python or third-party service calls from clients.
- Use HTTPS outside local development and configure CORS only for the deployed React origin and approved development origins.
- Store no secrets in React source, Flutter source, committed environment files or screenshots.
- Never log access tokens, refresh tokens, passwords, complete patient records or raw model prompts.
- Treat all API data as untrusted: validate response shape before rendering and handle missing optional fields.
- Use server-side authorization as the source of truth; client role guards are UX controls, not security controls.
- Sanitize or safely render audit summaries; never inject HTML from model/tool output.
- Use request cancellation and bounded retry behavior. Do not retry approval mutations automatically.
- Make status and risk labels text-based, keyboard accessible and screen-reader friendly.

## 9. Testing and acceptance evidence

### React tests

- Auth provider: login, logout, token expiry and role handling.
- Protected routes: unauthenticated redirect, non-admin denial and admin access.
- API client: success, validation error, 401, 403, 404, conflict, timeout and malformed response.
- Workflow queue: filters, pagination, loading, empty, refresh and retry states.
- Workflow detail: structured steps, audit entries, violations and redaction.
- Approval dialog: confirmation, required reason, disabled duplicate submission and successful refresh.
- Analytics/audit views: rendering and server-side filter parameters.
- Accessibility smoke checks for dialogs, tables, form labels and keyboard navigation.

### Flutter tests

- Model parsing and repository error mapping.
- Auth/token storage and route guarding.
- Emergency alert card/detail rendering for each risk and workflow status.
- Loading, empty, offline and retry states.
- Refresh after workflow status changes.
- Notification/deep-link behavior if implemented.
- Widget and navigation tests for the Component 4 feature.

### Integrated scenarios

1. Login as an operational user in Flutter and an Admin in React.
2. Create or load a synthetic emergency workflow through the API.
3. Confirm Flutter displays the emergency state.
4. Confirm React lists it under `PausedForApproval`.
5. Open details and verify plan, validation and audit data are structured.
6. Approve, reject or revise in React.
7. Confirm the API persists `AdminApproval` and the workflow status changes.
8. Refresh Flutter and verify the same status and workflow ID.
9. Verify an unauthorized user cannot approve or view admin-only data.
10. Capture screenshots or a short recording using synthetic data only.

## 10. Proposed implementation order

| Iteration | Tasks | Exit criteria |
|---|---|---|
| 1. Contract freeze | Confirm Swagger DTOs, role matrix, error format, polling/refresh strategy and React/Flutter state-management decisions | Typed client models and API examples are approved |
| 2. React foundation | Router, auth provider, API client, protected layout and error handling | Admin route protection and login flow work |
| 3. React workflow UI | Queue, metrics, detail, audit timeline and approval dialogs | Admin can review and act on a seeded paused workflow |
| 4. Flutter foundation | Component module, auth integration, models, repository and route guard | Operational user can reach protected workflow screens |
| 5. Flutter alert UI | Emergency list/detail, status refresh, offline/error states and selected device feature | Emergency workflow is clearly visible on mobile |
| 6. Cross-platform hardening | Authorization, accessibility, responsive layout, stale data, retry and redaction | UI behavior remains safe under errors and repeated actions |
| 7. Evidence and release | Automated tests, build artifacts, screenshots, demo script, README and deployment configuration | React build, Flutter APK and complete workflow demonstration are reproducible |

## 11. Definition of done

- [ ] React uses functional components, hooks, routing and a documented state-management approach.
- [ ] Flutter uses reusable widgets, routing, secure token storage and a documented state-management approach.
- [ ] Both clients use only ASP.NET Core APIs and the same user identity, permissions and workflow state.
- [ ] Admin dashboard lists and filters paused workflows and displays structured execution evidence.
- [ ] Admin can approve, reject or request revision with confirmation and visible result/error feedback.
- [ ] Flutter displays emergency/high-risk alerts and updated workflow statuses without exposing admin actions.
- [ ] Loading, empty, offline, unauthorized, validation, conflict and server-error states are handled.
- [ ] No client trusts free-form AI output for authorization or safety decisions.
- [ ] Tokens, secrets, hidden reasoning and unnecessary sensitive data are not rendered or logged.
- [ ] React and Flutter tests cover the main UI, API and authorization paths.
- [ ] One complete cross-platform workflow is demonstrated from mobile/API through agent review to web approval and mobile status update.
- [ ] React production build and Flutter APK can be generated from documented commands.

## 12. Risks and mitigations

| Risk | Mitigation |
|---|---|
| UI bypasses ASP.NET Core and calls Python directly | Keep Python internal and expose only API-backed client endpoints |
| Client shows stale or incorrect approval status | Refresh after mutations, use server response as truth and show stale-data indicators |
| Duplicate approvals or accidental rejection | Disable pending actions, require confirmation and never auto-retry approval mutations |
| Admin data leaks to operational users | Separate API DTOs/authorization policies; do not rely only on hidden UI controls |
| Emergency alert is missed or misunderstood | High-visibility layout, text/icon severity labels, timestamps, refresh behavior and accessible contrast |
| Model/tool output contains unsafe markup or sensitive content | Render typed summaries only, sanitize display fields and enforce backend redaction |
| Frontend dependencies destabilize the starter projects | Add only necessary dependencies, record versions and preserve existing scheduling screens |
| Push notification setup is unavailable during evaluation | Keep in-app refresh as the guaranteed path and document the optional notification adapter |

## 13. Phase 3 handoff and release evidence

The following must be ready for final integration and demonstration:

- React deployed URL configured against the deployed ASP.NET Core API.
- Flutter APK or approved runnable equivalent.
- Test accounts for Admin and operational roles with synthetic workflow fixtures.
- Swagger examples matching the client models.
- A sequence diagram showing Flutter/API -> ASP.NET Core -> Python agent -> PostgreSQL -> React approval -> Flutter status.
- Screenshots or recording of loading, paused, approval, rejection/revision, emergency and safe-failure states.
- Test output for React, Flutter, backend integration and the end-to-end scenario.
- README instructions for environment variables, startup order, API URL configuration and token setup.
- Evidence that both clients use the same workflow ID, permissions and persisted status.

