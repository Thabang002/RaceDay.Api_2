# RaceDay

.

```markdown
# RaceDay — Event Management & Race Timing System

A full-stack race-event management platform built with ASP.NET Core 10, Entity Framework Core, and SQL Server. RaceDay allows organisers to create and manage events, categories, and results, while participants can browse events, enrol in categories, and track their race results.

The system consists of three projects:

| Project | Purpose |
|---------|---------|
| RaceDay.Api | REST API — powers the entire system (Part 2) |
| RaceDay.Web | MVC front end consuming the API (Part 3) |
| RaceDay.Tests | xUnit integration & unit tests (Part 2 verification) |

---

## Table of Contents

1. [Features](#features)
2. [Tech Stack](#tech-stack)
3. [Solution Structure](#solution-structure)
4. [Getting Started](#getting-started)
5. [Configuration](#configuration)
6. [Database Setup](#database-setup)
7. [Running the API](#running-the-api)
8. [Running the Web Front End](#running-the-web-front-end)
9. [API Endpoints](#api-endpoints)
10. [Authentication & Roles](#authentication--roles)
11. [Testing](#testing)
12. [CI/CD](#cicd)
13. [Troubleshooting](#troubleshooting)
14. [License](#license)
15. [Author](#author)
16. [Acknowledgements](#acknowledgements)

---

## Features

### Participants
- Register and log in securely (JWT-based auth)
- Browse all public events with filters (date, location, status)
- View event details, categories, and weather forecast
- Enrol in event categories (with capacity checks)
- Withdraw from enrolments
- View personal race results
- View public leaderboards per event/category

### Organisers
- Create, update, and soft-delete events
- Manage locations
- Define race categories (distance, fee, capacity)
- View all enrolments per event
- Update payment statuses
- Capture and update race results (finish time, position, pace)
- View full event results ranked by position

### System
- Role-based access control (Organiser vs. Participant)
- Password hashing with BCrypt
- In-memory DB for tests (no SQL Server dependency during testing)
- Swagger UI for interactive API exploration
- Automated test suite (xUnit + WebApplicationFactory)
- GitHub Actions CI pipeline

---

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Language | C# 13 |
| Framework | .NET 10.0 |
| API | ASP.NET Core Web API |
| Web | ASP.NET Core MVC + Razor Views |
| ORM | Entity Framework Core 10 (Code-First) |
| Database | SQL Server (LocalDB for dev, InMemory for tests) |
| Auth | JWT Bearer tokens |
| Password Hashing | BCrypt.Net-Next |
| API Docs | Swashbuckle (Swagger UI) |
| Testing | xUnit, Microsoft.AspNetCore.Mvc.Testing |
| CI/CD | GitHub Actions |

---

## Solution Structure

```
RaceDay.sln
│
├── RaceDay.Api/                    # Part 2 — REST API
│   ├── Controllers/
│   │   ├── AuthController.cs
│   │   ├── UsersController.cs
│   │   ├── EventsController.cs
│   │   ├── CategoriesController.cs
│   │   ├── EnrolmentsController.cs
│   │   ├── ResultsController.cs
│   │   └── LocationsController.cs
│   ├── Models/
│   │   ├── Entities/               # DB entities (User, Event, ...)
│   │   └── DTOs/                   # Request/response contracts
│   ├── Data/
│   │   └── RaceDayDbContext.cs
│   ├── Services/
│   │   ├── ITokenService.cs
│   │   ├── TokenService.cs
│   │   ├── IPasswordHasher.cs
│   │   └── PasswordHasher.cs
│   ├── Middleware/
│   │   └── ExceptionHandlingMiddleware.cs
│   ├── Program.cs
│   ├── appsettings.json
│   └── RaceDay.Api.csproj
│
├── RaceDay.Web/                    # Part 3 — MVC front end
│   ├── Controllers/
│   │   ├── HomeController.cs
│   │   ├── AccountController.cs
│   │   ├── EventsController.cs
│   │   ├── EnrolmentsController.cs
│   │   └── ResultsController.cs
│   ├── Models/ViewModels/
│   ├── Services/
│   │   └── ApiClient.cs            # Typed HttpClient to the API
│   ├── Views/
│   │   ├── Shared/_Layout.cshtml
│   │   ├── Home/Index.cshtml
│   │   ├── Account/Login.cshtml
│   │   ├── Account/Register.cshtml
│   │   ├── Events/{Index,Details,Create,Edit,MyEvents}.cshtml
│   │   ├── Enrolments/{MyEnrolments,EventEnrolments}.cshtml
│   │   └── Results/{Leaderboard,MyResults,Manage}.cshtml
│   ├── wwwroot/
│   ├── Program.cs
│   └── RaceDay.Web.csproj
│
├── RaceDay.Tests/                  # Part 2 verification
│   ├── CustomWebApplicationFactory.cs
│   ├── TestHelpers.cs
│   ├── AuthTests.cs
│   ├── UsersTests.cs
│   ├── EventsTests.cs
│   ├── CategoriesTests.cs
│   ├── EnrolmentsTests.cs
│   ├── ResultsTests.cs
│   ├── LocationsTests.cs
│   ├── RoleAuthorizationTests.cs
│   └── RaceDay.Tests.csproj
│
└── .github/workflows/dotnet.yml    # CI pipeline
```

---

## Getting Started

### Prerequisites

- .NET 10 SDK
- SQL Server (LocalDB, Express, or full) — only needed for dev
- Visual Studio 2022+ or VS Code with C# Dev Kit
- Git

Verify your installs:

```bash
dotnet --version    # should print 10.x.x
git --version
```

### Clone

```bash
git clone https://github.com/<your-username>/raceday.git
cd raceday
```

### Restore & Build

```bash
dotnet restore RaceDay.sln
dotnet build   RaceDay.sln
```

---

## Configuration

### RaceDay.Api/appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=RaceDayDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "Jwt": {
    "Key": "ThisIsASuperSecretKeyForRaceDayApi_Min32Chars!",
    "Issuer": "RaceDayApi",
    "Audience": "RaceDayClient",
    "ExpiryMinutes": 120
  },
  "AllowedHosts": "*"
}
```

Never commit production secrets. Use `dotnet user-secrets` or environment variables in real deployments.

```bash
cd RaceDay.Api
dotnet user-secrets init
dotnet user-secrets set "Jwt:Key" "<your-long-secret>"
```

### RaceDay.Web/Program.cs

The ApiClient expects the API at `https://localhost:7000/`. Change this in `Services/ApiClient.cs` if your API runs on a different port.

---

## Database Setup

The API uses EF Core Code-First migrations.

### Apply migrations

```bash
cd RaceDay.Api
dotnet ef migrations add InitialCreate
dotnet ef database update
```

If `dotnet ef` isn't installed:

```bash
dotnet tool install --global dotnet-ef --version 10.*
```

### Reset the database (dev only)

```bash
dotnet ef database drop --force
dotnet ef database update
```

---

## Running the API

```bash
cd RaceDay.Api
dotnet run
```

- API root: https://localhost:7000
- Swagger UI: https://localhost:7000/swagger

### Explore with Swagger

1. Open https://localhost:7000/swagger
2. Click Authorize (top right)
3. Paste `Bearer <token>` after calling `/api/auth/login`
4. Try any endpoint directly from the browser

### Example — register and login

```bash
# Register a participant
curl -X POST https://localhost:7000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "alice@example.com",
    "password": "Password123!",
    "fullName": "Alice Smith",
    "role": "Participant"
  }'

# Login
curl -X POST https://localhost:7000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"alice@example.com","password":"Password123!"}'
```

---

## Running the Web Front End

In a separate terminal, keep the API running, then:

```bash
cd RaceDay.Web
dotnet run
```

- Web root: https://localhost:7001

The MVC app calls the API via ApiClient, forwarding the JWT stored in session.

---

## API Endpoints

All routes are prefixed with `/api`.

Legend:
- Public — no authentication required
- Auth — any authenticated user
- Self/Organiser — the owner user or an Organiser
- Organiser — Organiser role only
- Participant — Participant role only
- Organiser (owner) — Organiser who created the resource

### Authentication — /api/auth

| Method | Route | Access | Description |
|--------|-------|--------|-------------|
| POST | /register | Public | Register a new user |
| POST | /login | Public | Authenticate and receive JWT |
| POST | /logout | Auth | Invalidate session (client-side) |

### User Profile — /api/users

| Method | Route | Access | Description |
|--------|-------|--------|-------------|
| GET | /profile | Auth | Get current user's profile |
| PUT | /profile | Auth | Update profile |
| GET | /{id}/enrolments | Self/Organiser | Get user's enrolments |
| GET | /{id}/results | Self/Organiser | Get user's results |

### Events — /api/events

| Method | Route | Access | Description |
|--------|-------|--------|-------------|
| GET | / | Public | List all events (filters: date, locationId, status) |
| GET | /{id} | Public | Get event details |
| POST | / | Organiser | Create event |
| PUT | /{id} | Organiser (owner) | Update event |
| DELETE | /{id} | Organiser (owner) | Soft-delete event |
| GET | /{id}/categories | Public | List categories |
| GET | /{id}/enrolments | Organiser (owner) | List all enrolments |
| GET | /{id}/results | Organiser (owner) | Ranked results |
| GET | /{id}/weather | Public | Weather forecast |

### Categories — /api/categories & /api/events/{id}/categories

| Method | Route | Access | Description |
|--------|-------|--------|-------------|
| GET | /categories/{id} | Public | Get category |
| POST | /events/{id}/categories | Organiser (owner) | Create category |
| PUT | /categories/{id} | Organiser (owner) | Update category |
| DELETE | /categories/{id} | Organiser (owner) | Delete category |

### Enrolments — /api/enrolments & /api/events/{id}/enrol

| Method | Route | Access | Description |
|--------|-------|--------|-------------|
| POST | /events/{id}/enrol | Participant | Enrol in a category |
| GET | /enrolments/{id} | Self/Organiser | Get enrolment |
| PUT | /enrolments/{id} | Participant (self) | Update status (e.g. withdraw) |
| DELETE | /enrolments/{id} | Participant (self) | Cancel enrolment |
| PUT | /enrolments/{id}/payment | Organiser | Update payment status |

### Results — /api/results & /api/enrolments/{id}/results

| Method | Route | Access | Description |
|--------|-------|--------|-------------|
| POST | /enrolments/{id}/results | Organiser (owner) | Capture result |
| GET | /results/{id} | Self/Organiser | Get result |
| PUT | /results/{id} | Organiser (owner) | Update result |
| GET | /results/leaderboard | Public | Public leaderboard |

### Locations — /api/locations

| Method | Route | Access | Description |
|--------|-------|--------|-------------|
| GET | / | Public | List locations |
| GET | /{id} | Public | Get location |
| POST | / | Organiser | Create location |

Full interactive documentation is available at /swagger.

---

## Authentication & Roles

- JWT tokens issued on /api/auth/login and /api/auth/register
- Token contains: NameIdentifier (userId), Email, Name, Role
- Expiry: 120 minutes (configurable)
- Sent on every request as `Authorization: Bearer <token>`

### Role matrix

| Action | Participant | Organiser |
|--------|:-----------:|:---------:|
| Browse events | Yes | Yes |
| Enrol in event | Yes | No |
| Create event | No | Yes |
| Manage own event | No | Yes |
| Capture results | No | Yes (own event) |
| View own results | Yes | Yes |
| View any user's enrolments | No | Yes |

### Passwords

- Hashed with BCrypt before storage
- Never returned in any API response
- Minimum 6 characters (validated at registration)

---

## Testing

The test project spins up the API in-memory (via WebApplicationFactory) with an EF Core InMemory database. No SQL Server required.

### Run all tests

```bash
dotnet test RaceDay.Tests/RaceDay.Tests.csproj
```

### Run a specific test class

```bash
dotnet test --filter "FullyQualifiedName~AuthTests"
```

### Run a single test

```bash
dotnet test --filter "FullyQualifiedName~AuthTests.Register_ValidParticipant_Returns201WithToken"
```

### Generate coverage report

```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Test coverage

| Suite | Focus |
|-------|-------|
| AuthTests | Register, login, logout, validation |
| UsersTests | Profile get/update, user enrolments |
| EventsTests | Event CRUD, filters, ownership |
| CategoriesTests | Category CRUD, ownership |
| EnrolmentsTests | Enrol, capacity, withdraw, payment |
| ResultsTests | Capture, update, leaderboard |
| LocationsTests | Location CRUD |
| RoleAuthorizationTests | Cross-cutting role enforcement |

Expected result:

```
Passed!  - Failed: 0, Passed: 62, Skipped: 0, Total: 62
```

---

## CI/CD

A GitHub Actions workflow runs on every push and pull request.

### .github/workflows/dotnet.yml

```yaml
name: .NET 10 CI

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main, develop ]

jobs:
  build-and-test:
    runs-on: ubuntu-latest

    steps:
    - uses: actions/checkout@v4

    - name: Setup .NET 10
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '10.0.x'
        dotnet-quality: 'preview'

    - name: Restore
      run: dotnet restore RaceDay.sln

    - name: Build
      run: dotnet build RaceDay.sln -c Release --no-restore

    - name: Test
      run: dotnet test RaceDay.sln -c Release --no-build --verbosity normal

    - name: Upload test results
      if: always()
      uses: actions/upload-artifact@v4
      with:
        name: test-results
        path: '**/TestResults/*.trx'
```

### Pipeline stages

1. Checkout repository
2. Install .NET 10 SDK
3. Restore NuGet packages
4. Build in Release mode
5. Run the full test suite
6. Upload test result artifacts

The pipeline must be green before merging into main.

---

## Troubleshooting

### `dotnet ef` command not found

```bash
dotnet tool install --global dotnet-ef --version 10.*
export PATH="$PATH:$HOME/.dotnet/tools"   # Linux/macOS
```

### SQL Server connection errors

- Ensure SQL Server / LocalDB is running:
  ```bash
  sqllocaldb info
  sqllocaldb start mssqllocaldb
  ```
- Update ConnectionStrings:DefaultConnection in appsettings.Development.json

### 401 Unauthorized on protected endpoints

- Confirm token sent: `Authorization: Bearer <token>`
- Confirm token not expired (default 120 min)
- Confirm JWT Key/Issuer/Audience match between appsettings.json and TokenService

### 403 Forbidden on owner-only endpoints

- Confirm the JWT role claim matches the required role (Organiser)
- Confirm the resource's CreatedBy matches the current user

### CORS issues from the MVC app

- Confirm RaceDay.Web runs on https://localhost:7001
- Update the CORS policy in Program.cs if the port differs:
  ```csharp
  builder.Services.AddCors(o => o.AddPolicy("WebClient", p =>
      p.WithOrigins("https://localhost:7001").AllowAnyHeader().AllowAnyMethod()));
  ```

### Test project compilation errors

| Error | Fix |
|-------|-----|
| 'TestHelpers' does not contain a definition for 'JsonOpts' | Ensure TestHelpers exposes JsonOpts (not Json) |
| 'TestHelpers' does not contain a definition for 'EnsureLocationAsync' | Ensure the latest TestHelpers.cs is used |

### Reset everything

```bash
dotnet clean RaceDay.sln
rm -rf **/bin **/obj
dotnet restore RaceDay.sln
dotnet build   RaceDay.sln
```

---

## License

Educational project — The Independent Institute of Education (Pty) Ltd, 2026.

---

## Author

Name: <Your Name>  
Student Number: <your student number>  
Module: <module code>  
Institution: The Independent Institute of Education

---

## Acknowledgements

- Microsoft Learn — ASP.NET Core
- EF Core Docs
- BCrypt.Net-Next
- Swashbuckle.AspNetCore
```

---

## Where to Put This File

Save it as `README.md` at the root of your solution:

```
RaceDay/                     <- repo root
├── README.md                <- here
├── RaceDay.sln
├── RaceDay.Api/
├── RaceDay.Web/
├── RaceDay.Tests/
└── .github/workflows/dotnet.yml
```

GitHub will render it automatically on your repo's home page.

---

## Before You Commit

Fill in these placeholders in the Author section:

- `<Your Name>`
- `<your student number>`
- `<module code>`

