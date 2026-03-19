# TaskFlow

A RESTful Task Management API built with **ASP.NET Core 9** demonstrating clean architecture, JWT authentication, and Entity Framework Core.

## Tech Stack

- **ASP.NET Core 9** — Web API framework
- **Entity Framework Core 9** — ORM with SQL Server
- **ASP.NET Core Identity** — User management
- **JWT Bearer Authentication** — Access + refresh token flow
- **AutoMapper** — DTO mapping
- **Swagger / Scalar** — API documentation
- **xUnit** — Unit testing

## Architecture

The solution follows a clean, layered architecture:

```
TaskFlow/
├── TaskFlow.Core/             # Entities, interfaces, enums (no dependencies)
├── TaskFlow.Infrastructure/   # EF Core, Identity, service implementations
├── TaskFlow.API/              # Controllers, DTOs, middleware, static frontend
└── TaskFlow.Tests/            # xUnit unit tests
```

## Features

- User registration and login with JWT access tokens and refresh tokens
- Project management (create, update, delete, member management)
- Task management with priority and status tracking
- Comment threads on tasks
- Role-based access (Admin / Member)
- Global exception handling middleware
- Swagger UI for interactive API exploration
- Minimal static frontend (HTML/CSS/JS) served from wwwroot

## Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- SQL Server or LocalDB

### Setup

1. Clone the repo:
   ```bash
   git clone https://github.com/louay01/taskflow-dotnet.git
   cd taskflow-dotnet
   ```

2. Copy the example config and fill in your values:
   ```bash
   cp TaskFlow/TaskFlow.API/appsettings.Example.json TaskFlow/TaskFlow.API/appsettings.json
   ```

3. Update `appsettings.json` with your connection string, a strong JWT secret (min 32 chars), and seed admin credentials.

4. Apply migrations and run:
   ```bash
   cd TaskFlow/TaskFlow.API
   dotnet ef database update
   dotnet run
   ```

5. Open `https://localhost:{port}/swagger` to explore the API.

## Running Tests

```bash
cd TaskFlow/TaskFlow.Tests
dotnet test
```

## API Overview

| Resource     | Endpoints                                      |
|--------------|------------------------------------------------|
| Auth         | `POST /api/auth/register`, `POST /api/auth/login`, `POST /api/auth/refresh` |
| Projects     | `GET/POST /api/projects`, `GET/PUT/DELETE /api/projects/{id}` |
| Tasks        | `GET/POST /api/projects/{id}/tasks`, `PUT/DELETE /api/tasks/{id}` |
| Comments     | `GET/POST /api/tasks/{id}/comments`, `DELETE /api/comments/{id}` |
| Users        | `GET /api/users`, `GET /api/users/{id}` |
