# ⚡ ChannelCenter ASP.NET Core REST API

> **Core RESTful Backend Service** powered by ASP.NET Core 8 & Entity Framework Core.

---

## 📌 Overview

The **ChannelCenter REST API** serves as the central backend tier for the entire system. Both client applications (React Web & Flutter Mobile) communicate strictly with this API. It handles authentication, data persistence via PostgreSQL, business logic for doctor channeling, patient triage orchestration, and mutual internal communication with the Agentic AI Safety Auditor microservice.

---

## ✨ Core Functionality

- 🔒 **JWT Authentication & Authorization**: Role-based access control (Admin, Doctor, Nurse, Patient).
- 🐘 **Entity Framework Core Data Layer**: PostgreSQL data models with migrations and LINQ context mapping.
- 🩺 **Channeling & Appointment Engine**: Availability calculation, scheduling, ticket generation.
- 🚨 **Triage & Safety Audit Orchestration**: Internal HTTP dispatch to the Python AI Subsystem with internal secret validation.
- 📄 **Interactive OpenAPI / Swagger**: Built-in Swagger UI at `/swagger`.

---

## 🛠 Tech Stack

- **Framework**: C# / ASP.NET Core 8.0 Web API
- **ORM**: Entity Framework Core with Npgsql PostgreSQL provider
- **Database**: PostgreSQL 16
- **Auth**: JWT Bearer Tokens (`Microsoft.AspNetCore.Authentication.JwtBearer`)
- **Documentation**: Swashbuckle / OpenAPI

---

## 🚀 Local Development Setup

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download)
- PostgreSQL database instance or running Docker container

```bash
# 1. Navigate to API directory
cd src/backend/ChannelCenter.API

# 2. Restore NuGet dependencies
dotnet restore

# 3. Apply Entity Framework migrations
dotnet ef database update

# 4. Run API locally
dotnet run
```

The API will start listening at `http://localhost:5000` (or `http://localhost:5066`). Access Swagger documentation at `http://localhost:5000/swagger`.

---

## 🧪 Unit & Integration Testing

```bash
dotnet test
```
