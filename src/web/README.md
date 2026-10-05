# 🌐 ChannelCenter Web Application

> **React 19 + Vite Web Portal** for Clinical Administration, Doctor Scheduling & Staff Intake Management.

---

## 📌 Overview

The **ChannelCenter Web Application** provides the administrative and clinical management portal for the healthcare system. Built using **React 19** and **Vite**, it allows hospital administrators, doctors, and triage staff to manage patient intakes, register staff members, configure scheduling channels, and oversee automated safety audit logs.

---

## ✨ Features

- 🔑 **Role-Based Authentication**: Secure JWT login interface for Admin, Doctor, and Nurse roles.
- 📋 **Staff Registration & Management**: Admin portal for registering doctors and medical staff.
- 📅 **Doctor Availability & Scheduling**: Manage session times, slots, and doctor queue channels.
- 🛡️ **Safety Audit Log Portal**: Review flagged workflows, safety risk assessments, and clinical approvals.

---

## 🛠 Tech Stack

- **Framework**: React 19
- **Build Tool**: Vite 8
- **Testing**: Vitest + React Testing Library + jsdom
- **Linting**: ESLint 10

---

## 🚀 Local Development Setup

```bash
# 1. Navigate to web directory
cd src/web

# 2. Install dependencies
npm install

# 3. Start development server
npm run dev

# 4. Build for production
npm run build
```

The web app will run locally at `http://localhost:5173`.

---

## 🧪 Testing

```bash
# Run unit and integration tests
npm test

# Run tests in watch mode
npm run test:watch
```
