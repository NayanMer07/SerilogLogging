# 📋 OrderApi — Serilog Structured Logging in .NET 8

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat-square&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/en-us/download/dotnet/8)
[![Serilog](https://img.shields.io/badge/Serilog-4.0-14B8A6?style=flat-square)](https://serilog.net/)
[![Azure App Insights](https://img.shields.io/badge/Application%20Insights-Enabled-0078D4?style=flat-square&logo=microsoft-azure&logoColor=white)](https://azure.microsoft.com/en-us/products/monitor)
[![License: MIT](https://img.shields.io/badge/License-MIT-green?style=flat-square)](LICENSE)
[![CI/CD](https://img.shields.io/badge/CI%2FCD-GitHub%20Actions-2088FF?style=flat-square&logo=github-actions&logoColor=white)](/.github/workflows/deploy.yml)

A **production-ready ASP.NET Core Web API** demonstrating industry-standard structured logging with Serilog. Covers every essential pattern — structured message templates, multi-sink output, log level filtering, enrichers, correlation IDs, audit logging, and Azure Application Insights integration.

---

## 📋 Table of Contents

- [Overview](#-overview)
- [Why Structured Logging?](#-why-structured-logging)
- [Project Structure](#-project-structure)
- [API Endpoints](#-api-endpoints)
- [Logging Pipeline](#-logging-pipeline)
- [Log Levels](#-log-levels)
- [Prerequisites](#-prerequisites)
- [Getting Started](#-getting-started)
- [Configuration](#-configuration)
- [Running Locally](#-running-locally)
- [Testing Logging](#-testing-logging)
- [Deployment](#-deployment)
- [How to Utilize Serilog](#-how-to-utilize-serilog)
- [Architecture](#-architecture)
- [Best Practices Applied](#-best-practices-applied)
- [Contributing](#-contributing)

---

## 🌐 Overview

This project is an **Order Management API** with a complete Serilog structured logging pipeline. Every log event carries named properties — `OrderId`, `CustomerEmail`, `CorrelationId` — that can be filtered, grouped, and alerted on in any log sink, from the local console to Azure Application Insights.

**Key features:**
- ✅ Structured message templates — every property is queryable, not embedded in a string
- ✅ Multi-sink: Console, rolling File, Seq (local), Application Insights (Azure)
- ✅ Bootstrap logger — catches fatal startup exceptions before DI is ready
- ✅ `UseSerilogRequestLogging` — one structured log line per HTTP request
- ✅ `LogContext.PushProperty` — correlation IDs attached to every log in a request
- ✅ Custom `ILogEventEnricher` — auto-attaches authenticated user identity
- ✅ Dedicated audit log sink for compliance trails
- ✅ Per-sink minimum level filtering
- ✅ Framework noise suppression (`Microsoft.*`, `System.*` → Warning)
- ✅ Kusto queries for Azure Monitor alerting
- ✅ CI/CD via GitHub Actions

---

## ⚡ Why Structured Logging?

| | Traditional String Logging | Serilog Structured Logging |
|--|---------------------------|---------------------------|
| **Output** | `"Order 1042 placed by john@acme.com"` | `{ "OrderId": 1042, "CustomerEmail": "john@acme.com" }` |
| **Searchable** | ❌ Full-text only | ✅ Filter by `OrderId = 1042` |
| **Alertable** | ❌ Regex patterns | ✅ `WHERE ErrorCount > 10` |
| **Queryable** | ❌ Grep / string match | ✅ Group by `CustomerEmail` |
| **Performance** | Slower (string concat) | Faster (deferred rendering) |

Structured logging turns your logs into **queryable data** — not just text.

---

## 📁 Project Structure

```
OrderApi/
├── .github/
│   └── workflows/
│       └── deploy.yml                  # GitHub Actions CI/CD pipeline
├── Controllers/
│   └── OrdersController.cs             # HTTP endpoints with LogContext enrichment
├── Services/
│   ├── IOrderService.cs                # Service interface
│   └── OrderService.cs                 # Business logic with structured log calls
├── Middleware/
│   └── CorrelationMiddleware.cs        # Attach CorrelationId to every request log
├── Enrichers/
│   └── UserContextEnricher.cs          # Custom ILogEventEnricher for user identity
├── Program.cs                          # Bootstrap logger + UseSerilog() setup
├── appsettings.json                    # Serilog config — sinks, levels, enrichers
├── appsettings.Development.json        # Dev overrides — Seq sink, Debug level
├── appsettings.Production.json         # Prod overrides — AppInsights only, Info level
├── logs/                               # Rolling log files (in .gitignore)
├── OrderApi.csproj                     # NuGet packages
└── README.md                           # This file
```

---

## 🔌 API Endpoints

Base URL (local): `https://localhost:5001/api`

| Method | Endpoint | Description | Logs Emitted |
|--------|----------|-------------|-------------|
| `POST` | `/api/orders` | Place a new order | Info: order placed, scope with OrderId |
| `GET` | `/api/orders/{id}` | Get order by ID | Debug: fetching order |
| `DELETE` | `/api/orders/{id}` | Cancel an order | Warning: order cancelled + reason |


Every request also emits one structured `HTTP {Method} {Path} responded {StatusCode} in {Elapsed} ms` log via `UseSerilogRequestLogging`.

---

## 🔄 Logging Pipeline

```
Log Event (_logger.LogInformation(...))
         │
         ▼
   ┌─────────────┐
   │  Enrichers  │  ← MachineeName, ThreadId, CorrelationId, UserId, Application
   └──────┬──────┘
          │
          ▼
   ┌─────────────┐
   │   Filters   │  ← Minimum level per namespace (Microsoft → Warning)
   └──────┬──────┘
          │
   ┌──────┼──────────────────┐
   ▼      ▼                  ▼
Console  File (rolling)  Application Insights
(Debug+) (Warning+)      (Information+)
```

---

## 📊 Log Levels

| Level | When to Use | Production? |
|-------|-------------|-------------|
| `Verbose` | Algorithm internals, loop iterations | ❌ Never |
| `Debug` | Variable values, branch decisions | Dev/Staging only |
| `Information` | User actions, order placed, login success | ✅ Yes |
| `Warning` | Retry succeeded, deprecated API used | ✅ Yes — alert on spike |
| `Error` | Unhandled exception, DB failure, API error | ✅ Yes — alert immediately |
| `Fatal` | App cannot continue, data corruption risk | ✅ Yes — wake someone up |

---

## 🛠 Prerequisites

| Tool | Version | Download |
|------|---------|----------|
| .NET SDK | 8.0+ | [dot.net/download](https://dotnet.microsoft.com/download) |
| Visual Studio Code | Latest | [code.visualstudio.com](https://code.visualstudio.com) |
| Docker Desktop | Latest (for Seq) | [docker.com](https://www.docker.com/products/docker-desktop) |
| Azure Account | Free | [portal.azure.com](https://portal.azure.com) |

**Recommended VS Code Extensions:**
- `ms-dotnettools.csdevkit` — C# Dev Kit
- `humao.rest-client` — HTTP testing
- `ms-azuretools.vscode-docker` — Docker integration

**NuGet Packages Used:**

```bash
Serilog.AspNetCore                       # Core ASP.NET Core integration
Serilog.Sinks.Console                    # Terminal output
Serilog.Sinks.File                       # Rolling file output
Serilog.Sinks.Seq                        # Local structured log viewer
Serilog.Sinks.ApplicationInsights        # Azure cloud telemetry
Serilog.Enrichers.Environment            # MachineName, etc.
Serilog.Enrichers.Thread                 # ThreadId
Serilog.Enrichers.Process                # ProcessId
```

---

## 🚀 Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/your-username/OrderApi.git
cd OrderApi
```

### 2. Restore Dependencies

```bash
dotnet restore
```

### 3. Start Seq (Optional — Local Log Viewer)

```bash
# Run Seq in Docker — best way to explore structured logs locally
docker run -d \
  --name seq \
  -e ACCEPT_EULA=Y \
  -p 5341:80 \
  datalust/seq

# Open http://localhost:5341 to view logs
```

### 4. Run the API

```bash
dotnet run
# API:     https://localhost:5001
# Swagger: https://localhost:5001/swagger
# Seq:     http://localhost:5341
```

---

## ⚙️ Configuration

Serilog is fully configured via `appsettings.json` — no code changes needed to swap sinks or adjust levels.

### appsettings.json (shared base)

```json
{
  "Serilog": {
    "Using": ["Serilog.Sinks.Console", "Serilog.Sinks.File"],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.Hosting.Lifetime": "Information",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/orderapi-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 7
        }
      }
    ],
    "Enrich": ["FromLogContext", "WithMachineName", "WithThreadId"],
    "Properties": {
      "Application": "OrderApi"
    }
  }
}
```

### appsettings.Production.json

```json
{
  "Serilog": {
    "Using": ["Serilog.Sinks.ApplicationInsights"],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "Properties": {
      "Application": "OrderApi",
      "Environment": "Production"
    }
  }
}
```

> The Application Insights connection string is injected at runtime via **Azure App Settings** — never hardcoded in config files.

---

## 💻 Running Locally

```bash
# Start the API
dotnet run

# Debug mode with breakpoints
# Press F5 in VS Code
```

**What you'll see in the console:**

```
[10:23:38 INF] Starting OrderApi
[10:23:39 INF] Now listening on: https://localhost:5001
[10:23:41 INF] HTTP POST /api/orders responded 201 in 52.3 ms { StatusCode: 201 }
[10:23:41 INF] Placing order 1042 for john@acme.com total $249.99 { OrderId: 1042 }
[10:23:41 INF] Order 1042 confirmed successfully { OrderId: 1042, CorrelationId: "abc123" }
[10:23:55 WRN] Order 1087 cancelled — Reason: OutOfStock { OrderId: 1087 }
```

---

## 🧪 Testing Logging

### Trigger Log Events with curl

```bash
# POST — triggers Information + scope logs
curl -X POST https://localhost:5001/api/orders \
  -H "Content-Type: application/json" \
  -d '{"id":1042,"customerEmail":"john@acme.com","total":249.99,"status":"Pending"}'

# GET — triggers Debug log
curl https://localhost:5001/api/orders/1042

# Cancel — triggers Warning log
curl -X DELETE https://localhost:5001/api/orders/1042?reason=OutOfStock

# Check log file
cat logs/orderapi-$(date +%Y%m%d).log
```

### Query Logs in Seq (http://localhost:5341)

```sql
-- Filter by structured property
OrderId = 1042

-- Show all errors in the last hour
@Level = 'Error' and @Timestamp > Now() - 1h

-- Count orders by customer
select count(*) from stream
where @MessageTemplate like '%order%'
group by CustomerEmail

-- Trace a full request by CorrelationId
CorrelationId = 'abc123def456'
```

> Structured queries like `OrderId = 1042` are **impossible** with traditional string logging — this is the core value of Serilog.

### VS Code REST Client

Create a `test.http` file:

```http
### Place an order
POST https://localhost:5001/api/orders
Content-Type: application/json

{
  "id": 1042,
  "customerEmail": "john@acme.com",
  "total": 249.99,
  "status": "Pending"
}

### Get order
GET https://localhost:5001/api/orders/1042

### Cancel order
DELETE https://localhost:5001/api/orders/1042?reason=OutOfStock
```

---

## ☁️ Deployment

### Option A — Azure CLI

```bash
# Login
az login

# Create resource group
az group create --name OrderApiRG --location eastus

# Create Application Insights
az monitor app-insights component create \
  --app orderapi-insights \
  --location eastus \
  --resource-group OrderApiRG \
  --kind web

# Get connection string
az monitor app-insights component show \
  --app orderapi-insights \
  --resource-group OrderApiRG \
  --query connectionString -o tsv

# Create App Service
az appservice plan create \
  --name OrderApiPlan \
  --resource-group OrderApiRG \
  --sku F1 --is-linux

az webapp create \
  --name orderapi-yourname \
  --resource-group OrderApiRG \
  --plan OrderApiPlan \
  --runtime "DOTNETCORE:8.0"

# Inject secrets via App Settings (never hardcode!)
az webapp config appsettings set \
  --name orderapi-yourname \
  --resource-group OrderApiRG \
  --settings \
    "ApplicationInsights__ConnectionString=InstrumentationKey=...;IngestionEndpoint=..." \
    "ASPNETCORE_ENVIRONMENT=Production" \
    "Serilog__MinimumLevel__Default=Information"

# Build and publish
dotnet publish -c Release -o ./publish
az webapp deployment source config-zip \
  --src ./publish.zip \
  --name orderapi-yourname \
  --resource-group OrderApiRG
```

### Option B — GitHub Actions (Recommended)

The pipeline is pre-configured in `.github/workflows/deploy.yml`. Automatically deploys on every push to `main`.

**One-time setup:**
1. Azure Portal → Your Web App → **Get publish profile** → Download
2. GitHub → **Settings → Secrets → Actions** → New secret
3. Name: `AZURE_WEBAPP_PUBLISH_PROFILE`, Value: paste the downloaded content

Every `git push` to `main` triggers an automatic build and deploy. ✅

---

## 📡 How to Utilize Serilog

### Structured Log Calls

```csharp
// ✅ Named properties — queryable in any sink
_logger.LogInformation(
    "Order {OrderId} placed by {CustomerEmail} total {Total:C}",
    order.Id, order.CustomerEmail, order.Total);

// ❌ String interpolation — destroys structure
_logger.LogInformation($"Order {order.Id} placed by {order.CustomerEmail}");
```

### Log Scopes — Push Context to All Logs in a Block

```csharp
using var scope = _logger.BeginScope(new Dictionary<string, object>
{
    ["OrderId"]       = order.Id,
    ["CustomerEmail"] = order.CustomerEmail
});

// All logs within this using block automatically include OrderId and CustomerEmail
_logger.LogInformation("Processing payment");
_logger.LogInformation("Sending confirmation email");
```

### LogContext — Per-Request Properties

```csharp
// In middleware — attaches CorrelationId to every log in the request
using (LogContext.PushProperty("CorrelationId", HttpContext.TraceIdentifier))
using (LogContext.PushProperty("UserId", user.Identity.Name))
{
    await next(context);  // all logs in this request include CorrelationId + UserId
}
```

### Azure Monitor Queries (Kusto)

```kusto
// Find all error logs for a specific order
traces
| where severityLevel >= 3
| where customDimensions.OrderId == "1042"
| project timestamp, message, customDimensions
| order by timestamp desc

// Alert: more than 10 errors in 5 minutes
traces
| where severityLevel == 3
| summarize ErrorCount = count() by bin(timestamp, 5m)
| where ErrorCount > 10

// Trace a request end-to-end by CorrelationId
traces
| where customDimensions.CorrelationId == "abc123def456"
| order by timestamp asc
```

---

## 🏗 Architecture

```
HTTP Request
     │
     ▼
CorrelationMiddleware  ←── Generates/reads X-Correlation-ID header
     │                     Pushes CorrelationId + UserId to LogContext
     ▼
OrdersController       ←── Pushes additional context, calls service
     │
     ▼
OrderService           ←── Structured log calls with named properties
     │
     ▼
Serilog Pipeline
     ├── Enrichers: MachineName, ThreadId, CorrelationId, Application
     ├── Filter: MinimumLevel per namespace
     └── Sinks:
          ├── Console (Development)
          ├── File rolling (local backup)
          ├── Seq (local query UI)
          └── Application Insights (Azure production)
```

---

## ✅ Best Practices Applied

| Practice | Implementation |
|----------|---------------|
| **Message templates always** | `{OrderId}` never `$"{id}"` — preserves structure |
| **Bootstrap logger first** | `CreateBootstrapLogger()` before DI — catches startup crashes |
| **CloseAndFlush in finally** | Guarantees buffered events are flushed before process exit |
| **Suppress framework noise** | `Microsoft.*` and `System.*` → Warning in all environments |
| **Correlation IDs** | Middleware pushes to `LogContext` so every log in the request carries it |
| **Per-sink level filtering** | Console = Debug, File = Warning, AppInsights = Information |
| **No secrets in code** | AppInsights connection string via Azure App Settings only |
| **Separate audit sink** | Compliance-grade audit log separate from application logs |
| **Different levels per env** | Debug in dev, Information in staging, Warning in prod |
| **CI/CD pipeline** | GitHub Actions deploys on push to main — no manual deploys |

---

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/your-feature`
3. Commit your changes: `git commit -m 'feat: add your feature'`
4. Push to the branch: `git push origin feature/your-feature`
5. Open a Pull Request

---


<div align="center">
  Built with 📋 Serilog · .NET 8 · Azure Application Insights · Seq
</div>
