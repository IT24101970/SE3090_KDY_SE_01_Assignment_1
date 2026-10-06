# SE3110: Defect & Bug Report Document
**Project Name:** SE3090 Integrated ChannelCenter System  
**Document Purpose:** Log defects identified during automated test execution, root cause analysis, code fixes, and retesting evidence.  

---

## Defect Summary Overview

| Defect ID | Title / Description | System Component | Severity | Priority | Original Status | Retest Status |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| **BUG-01** | `CreateAppointment` unauthenticated request returns 400 instead of 401 | Backend API (`AppointmentsController`) | High | High | Failed | ✅ RESOLVED |
| **BUG-02** | `SlotBookingModal` test payload mismatch for `skipAiWorkflows` | React Web (`SlotBookingModal.test.jsx`) | Medium | High | Failed | ✅ RESOLVED |
| **BUG-03** | `AppointmentDirectoryTab` query title mismatch on Cancel button | React Web (`AppointmentDirectoryTab.test.jsx`) | Low | Medium | Failed | ✅ RESOLVED |

---

## Detailed Bug Reports & Resolution Evidence

### Bug ID: BUG-01 — Unauthenticated Appointment Creation Response Status Code Mismatch
- **Severity:** High  
- **Component:** ASP.NET Core API (`src/backend/ChannelCenter.API/Controllers/Appointment/AppointmentsController.cs`)  
- **Test Case Ref:** `TC-API-02` (`AppointmentsControllerTests.CreateAppointment_UnauthenticatedUser_ReturnsUnauthorized`)  

#### 1. Symptom & Failure Description
When an unauthenticated request (without user identity claims) was posted to `POST /api/appointments`, the controller bypassed authorization validation and passed the DTO directly to `AppointmentService.CreateAppointmentAsync()`. When entity lookups failed, it returned `400 Bad Request` instead of `401 Unauthorized`.

```
Failed CASE 2 — Unauthenticated user: appointment creation rejected with 401 Unauthorized
Expected: typeof(Microsoft.AspNetCore.Mvc.UnauthorizedObjectResult)
Actual:   typeof(Microsoft.AspNetCore.Mvc.BadRequestObjectResult)
```

#### 2. Root Cause Analysis
The controller logic in `AppointmentsController.cs` checked `if (HttpContext.User != null && HttpContext.User.Identity?.IsAuthenticated == true)` to validate authorization, but had no `else` block to reject unauthenticated calls.

#### 3. Code Fix Implemented
Updated `AppointmentsController.cs` to explicitly return `Unauthorized()` when `HttpContext.User` is null or `IsAuthenticated` is false:

```csharp
// Refactored in AppointmentsController.cs
if (HttpContext.User == null || HttpContext.User.Identity?.IsAuthenticated != true)
{
    return Unauthorized(new { message = "User is unauthenticated." });
}
```

#### 4. Retest Evidence
- **Command Executed:** `dotnet test src/backend/ChannelCenter.Tests/ChannelCenter.Tests.csproj`
- **Result:** **Passed! 87 Passed, 0 Failed.**

---

### Bug ID: BUG-02 — Missing `skipAiWorkflows` Flag in React Web Booking Payload Assertion
- **Severity:** Medium  
- **Component:** React Web Portal (`src/web/src/test/SlotBookingModal.test.jsx`)  
- **Test Case Ref:** `TC-WEB-03`  

#### 1. Symptom & Failure Description
When executing Vitest suite `SlotBookingModal.test.jsx`, the test failed with assertion mismatch:
```
expect(appointmentApi.create).toHaveBeenCalledWith(expectedPayload)
```
The actual function call included `skipAiWorkflows: true`, whereas the test specification omitted this property.

#### 2. Root Cause Analysis
`SlotBookingModal.jsx` was updated during AI integration to bypass secondary AI triggers for direct manual channel bookings, but `SlotBookingModal.test.jsx` expectation was not synchronized.

#### 3. Code Fix Implemented
Updated assertion in `SlotBookingModal.test.jsx`:

```javascript
await waitFor(() => {
  expect(appointmentApi.create).toHaveBeenCalledWith({
    patientId: 3,
    doctorId: 2,
    scheduleId: 9,
    appointmentDate: '2026-09-29T13:45:00Z',
    reasonForVisit: 'Severe migraine and nausea for two days',
    skipAiWorkflows: true,
  });
});
```

#### 4. Retest Evidence
- **Command Executed:** `npm --prefix src/web run test`
- **Result:** **Passed! `SlotBookingModal.test.jsx` 3/3 passed.**

---

### Bug ID: BUG-03 — Cancel Button Title Attribute Matcher Mismatch in Appointment Directory
- **Severity:** Low  
- **Component:** React Web Portal (`src/web/src/test/AppointmentDirectoryTab.test.jsx`)  
- **Test Case Ref:** `TC-WEB-04`  

#### 1. Symptom & Failure Description
Vitest suite failed with:
```
Object.getElementError: Unable to find an element with title: "Cancel booking with required reason"
```

#### 2. Root Cause Analysis
The UI component `AppointmentDirectoryTab.jsx` renders `title="Cancel appointment booking"` on table row cancel action buttons. The test file queried for `"Cancel booking with required reason"`.

#### 3. Code Fix Implemented
Updated line 142 in `AppointmentDirectoryTab.test.jsx`:
```javascript
const cancelBtn = screen.getAllByTitle('Cancel appointment booking')[0];
```

#### 4. Retest Evidence
- **Command Executed:** `npm --prefix src/web run test`
- **Result:** **Passed! All 6 test files passed (51/51 tests).**
