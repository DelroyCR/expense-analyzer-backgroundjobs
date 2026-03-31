# ExpenseAnalyzer.BackgroundJobs

A focused background processing project built with **ASP.NET Core**, **Entity Framework Core**, and **PostgreSQL**.

This project complements the main **Expense Analyzer** API by reading existing transaction data and generating **daily summary snapshots** through:

- a **manual trigger endpoint**
- an **automatic scheduler**
- persistent **job execution logs**

The goal of this project is to demonstrate a small vertical slice for **background processing**, including scheduling, persistence, configuration, and test coverage.

---

## Overview

`ExpenseAnalyzer.BackgroundJobs` is a separate backend solution designed to process existing financial transaction data and generate daily aggregates per user.

It uses the same PostgreSQL database as the main Expense Analyzer application, but with a clear separation of responsibilities:

- it **reads** from the existing `Transactions` table
- it **writes** to its own tables:
  - `SummarySnapshots`
  - `BackgroundJobExecutions`

---

## Features

- Generate daily summary snapshots from existing transactions
- Persist execution logs for every background job run
- Support **manual execution** through an API endpoint
- Support **automatic execution** through a hosted background scheduler
- Configure scheduler behavior from `appsettings.json`
- Track whether execution was triggered by:
  - `Manual`
  - `Scheduler`
- Unit test coverage for the main service flow

---

## What the project does

For a given UTC date, the service:

1. Creates a `BackgroundJobExecution` record with status `Running`
2. Loads transactions for that day
3. Groups transactions by user
4. Calculates:
   - `IncomeTotal`
   - `ExpenseTotal`
   - `NetTotal`
   - `TransactionCount`
5. Creates or updates the corresponding `SummarySnapshot`
6. Marks the execution as `Succeeded`
7. If an error occurs, marks the execution as `Failed`

---

## Architecture

The solution follows a clean layered structure:

- **ExpenseAnalyzer.BackgroundJobs.Api**
  - API endpoints
  - DI registration
  - hosted background scheduler
  - configuration binding

- **ExpenseAnalyzer.BackgroundJobs.Application**
  - DTOs
  - service contracts
  - business logic
  - repository abstractions

- **ExpenseAnalyzer.BackgroundJobs.Domain**
  - core entities

- **ExpenseAnalyzer.BackgroundJobs.Infrastructure**
  - EF Core DbContext
  - Fluent API configurations
  - repository implementations
  - persistence concerns

- **ExpenseAnalyzer.BackgroundJobs.UnitTests**
  - unit tests for application logic

---

## Solution structure

```text
ExpenseAnalyzer.BackgroundJobs/
├── ExpenseAnalyzer.BackgroundJobs.Api/
├── ExpenseAnalyzer.BackgroundJobs.Application/
├── ExpenseAnalyzer.BackgroundJobs.Domain/
├── ExpenseAnalyzer.BackgroundJobs.Infrastructure/
└── ExpenseAnalyzer.BackgroundJobs.UnitTests/
```

---

## Tech stack

- **.NET 10**
- **ASP.NET Core Web API**
- **Entity Framework Core**
- **PostgreSQL**
- **Swashbuckle / Swagger UI**
- **Hosted BackgroundService**
- **xUnit** for testing

---

## Domain entities

### SourceTransaction

Represents the source data already stored in the shared database.

```csharp
public class SourceTransaction
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
}
```

### SummarySnapshot

Represents the calculated daily summary per user.

```csharp
public class SummarySnapshot
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    public DateTime PeriodStartUtc { get; set; }
    public DateTime PeriodEndUtc { get; set; }

    public decimal IncomeTotal { get; set; }
    public decimal ExpenseTotal { get; set; }
    public decimal NetTotal { get; set; }

    public int TransactionCount { get; set; }

    public DateTime GeneratedAtUtc { get; set; }
}
```

### BackgroundJobExecution

Stores the execution log for each run.

```csharp
public class BackgroundJobExecution
{
    public Guid Id { get; set; }

    public string JobName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;

    public DateTime StartedAtUtc { get; set; }
    public DateTime? FinishedAtUtc { get; set; }

    public string? ErrorMessage { get; set; }

    public int AttemptCount { get; set; }

    public string TriggeredBy { get; set; } = string.Empty;
}
```

---

## Database design

This project uses the **same PostgreSQL database** as the main Expense Analyzer application.

### Reads from

- `Transactions`

### Writes to

- `SummarySnapshots`
- `BackgroundJobExecutions`

`Transactions` is mapped for reading only and excluded from this project's migrations.

---

## Main application service

The core use case is handled by:

- `IGenerateSummarySnapshotsService`
- `GenerateSummarySnapshotsService`

Current method signature:

```csharp
Task<GenerateDailySummariesResultDto> GenerateDailyAsync(
    GenerateDailySummariesRequestDto request,
    string triggeredBy = "Manual",
    CancellationToken cancellationToken = default);
```

### Request DTO

```csharp
public class GenerateDailySummariesRequestDto
{
    public DateTime DateUtc { get; set; }
    public Guid? UserId { get; set; }
}
```

### Result DTO

```csharp
public class GenerateDailySummariesResultDto
{
    public Guid ExecutionId { get; set; }
    public string JobName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int ProcessedUsers { get; set; }
    public int GeneratedSnapshots { get; set; }
    public DateTime DateUtc { get; set; }
}
```

---

## API endpoint

The project exposes a manual trigger endpoint:

```http
POST /api/jobs/generate-daily-summaries
```

### Example request body

```json
{
  "dateUtc": "2026-03-30T00:00:00Z",
  "userId": null
}
```

### What it does

- validates the request
- runs the daily summary generation flow
- logs the execution as `Manual`
- returns execution details

---

## Background scheduler

The project includes a hosted scheduler implemented with `BackgroundService`.

It can:

- run automatically on startup
- run repeatedly using a configured interval
- process a configurable target day using an offset
- log executions as `Scheduler`

---

## Scheduler configuration

The scheduler is configured from `appsettings.json` using a `SummaryScheduler` section.

### Example

```json
"SummaryScheduler": {
  "Enabled": true,
  "RunOnStartup": true,
  "IntervalSeconds": 60,
  "ProcessDateOffsetDays": -1
}
```

### Configuration fields

| Field | Description |
|---|---|
| `Enabled` | Enables or disables the scheduler |
| `RunOnStartup` | Executes once when the API starts |
| `IntervalSeconds` | Interval between executions |
| `ProcessDateOffsetDays` | Relative day to process (for example `-1` = yesterday) |

### Common examples

#### Disable scheduler

```json
"SummaryScheduler": {
  "Enabled": false,
  "RunOnStartup": false,
  "IntervalSeconds": 60,
  "ProcessDateOffsetDays": -1
}
```

#### Run on startup and every 15 seconds

```json
"SummaryScheduler": {
  "Enabled": true,
  "RunOnStartup": true,
  "IntervalSeconds": 15,
  "ProcessDateOffsetDays": -1
}
```

#### Wait for first interval instead of running immediately

```json
"SummaryScheduler": {
  "Enabled": true,
  "RunOnStartup": false,
  "IntervalSeconds": 60,
  "ProcessDateOffsetDays": -1
}
```

---

## Getting started

### Prerequisites

- .NET 10 SDK
- PostgreSQL
- Existing Expense Analyzer database with `Transactions` table already present

---

## Configuration

### 1. Clone the repository

```bash
git clone <your-repository-url>
cd expense-analyzer-backgroundjobs
```

### 2. Restore dependencies

```bash
dotnet restore
```

### 3. Set the connection string

Use **User Secrets** for local development.

From the `ExpenseAnalyzer.BackgroundJobs.Api` project:

```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=YourDatabaseName;Username=your_user;Password=your_password"
```

---

## Migrations

This project should create only its own tables:

- `SummarySnapshots`
- `BackgroundJobExecutions`

Apply migrations with:

```bash
dotnet ef database update --project ExpenseAnalyzer.BackgroundJobs.Infrastructure --startup-project ExpenseAnalyzer.BackgroundJobs.Api
```

---

## Run the API

```bash
dotnet run --project ExpenseAnalyzer.BackgroundJobs.Api
```

If everything is configured correctly, Swagger UI should be available for testing the manual endpoint.

---

## Run tests

```bash
dotnet test
```

---

## Example execution flow

### Manual trigger

1. Call `POST /api/jobs/generate-daily-summaries`
2. A new `BackgroundJobExecution` row is created
3. Transactions for the requested date are loaded
4. Snapshots are created or updated
5. Execution status becomes `Succeeded`

### Scheduler trigger

1. Application starts
2. Scheduler reads settings from configuration
3. If enabled, it executes automatically
4. Execution is logged with `TriggeredBy = "Scheduler"`
5. Snapshots are generated for the configured target day

---

## Example SQL checks

After running the project, you can validate behavior directly in PostgreSQL.

### Job executions

```sql
SELECT *
FROM "BackgroundJobExecutions"
ORDER BY "StartedAtUtc" DESC;
```

### Summary snapshots

```sql
SELECT *
FROM "SummarySnapshots"
ORDER BY "GeneratedAtUtc" DESC;
```

---

## Testing strategy

This project includes focused unit testing for the core business flow.

A key test validates that when transactions exist and no prior snapshot is found, the service:

- creates a snapshot
- logs the background job execution
- returns a successful result

---

## Status

Completed as a small background processing project with:

- manual trigger
- configurable scheduler
- execution logging
- summary snapshot generation
- unit test coverage

---

## License

This project is for portfolio and educational purposes unless otherwise specified.
