# ENV Instruction — Frontend ↔ Backend API Connection

## Overview

This project has two parts:

| Part         | Tech Stack                          | Location                           |
| :----------- | :---------------------------------- | :--------------------------------- |
| **Frontend** | WinUI 3 / Uno Platform (C#)        | `CSC13001-my-shop-project/`        |
| **Backend**  | Node.js + Express + Apollo GraphQL  | `backend/`                         |

The backend exposes a **GraphQL API** at `http://localhost:4000/graphql`.

> [!IMPORTANT]
> The frontend is a .NET desktop app — it does **NOT** use a `.env` file.
> Instead, it uses `appsettings.json` and `appsettings.development.json` for configuration.

---

## Backend Setup (`.env` file)

The backend **does** use a `.env` file. Copy the template and fill in your values:

```bash
cd backend
cp .env.example .env
```

### Required `.env` variables

```dotenv
NODE_ENV=development
PORT=4000

# PostgreSQL
DB_HOST=localhost
DB_PORT=5432
DB_USER=postgres
DB_PASSWORD=yourpassword
DB_NAME=myshop

# JWT Authentication
JWT_SECRET=your_super_secret_key_here
JWT_EXPIRES_IN=24h

# Cloudinary (product image hosting)
CLOUDINARY_CLOUD_NAME=your_cloud_name_here
CLOUDINARY_API_KEY=your_api_key_here
CLOUDINARY_API_SECRET=your_api_secret_here

# pgAdmin (optional, for DB management UI)
PGADMIN_EMAIL=admin@myshop.dev
PGADMIN_PASSWORD=admin

# Elasticsearch
ELASTICSEARCH_NODE=http://elasticsearch:9200
ELASTICSEARCH_PRODUCT_INDEX=products

# Redis
REDIS_HOST=redis
REDIS_PORT=6379
# REDIS_PASSWORD=         # Optional
```

### Start the backend

```bash
cd backend
docker compose up -d
```

The GraphQL API will be available at: **`http://localhost:4000/graphql`**

---

## Frontend Setup (No `.env` — Uses `appsettings.json`)

### Why no `.env`?

The Uno Platform / WinUI 3 app uses the .NET configuration system:

- Configuration files (`appsettings.json`) are **embedded as resources** at build time.
- They are loaded via `.UseConfiguration()` in `App.xaml.cs`.
- This is the standard .NET pattern — **`.env` files are a Node.js convention**, not applicable here.

### Configuration files

| File                          | Environment  | Purpose                              |
| :---------------------------- | :----------- | :----------------------------------- |
| `appsettings.json`            | Production   | Default settings                     |
| `appsettings.development.json`| Development  | Overrides for local development      |

### Step 1 — Set the Backend API URL

Edit `appsettings.development.json` to point to your backend:

```json
{
  "AppConfig": {
    "Environment": "Development"
  },
  "ApiClient": {
    "Url": "http://localhost:4000",
    "UseNativeHandler": true
  }
}
```

> [!NOTE]
> The `ApiClient:Url` value must match the backend's `PORT` in your `.env` file.
> Default: `http://localhost:4000`

For production, update `appsettings.json` with the deployed API URL:

```json
{
  "ApiClient": {
    "Url": "https://your-production-api.com"
  }
}
```

### Step 2 — Register HttpClient in `App.xaml.cs`

In the `.UseHttp()` section of `App.xaml.cs`, register a named HttpClient that reads the URL from config:

```csharp
.UseHttp((context, services) =>
{
    // Register a named HttpClient for backend API calls
    services.AddHttpClient("BackendApi", client =>
    {
        var baseUrl = context.Configuration["ApiClient:Url"]
                      ?? "http://localhost:4000";
        client.BaseAddress = new Uri(baseUrl);
        client.DefaultRequestHeaders.Add("Accept", "application/json");
    });

#if DEBUG
    services.AddTransient<DelegatingHandler, DebugHttpHandler>();
#endif
})
```

### Step 3 — Create a GraphQL Service

Since the backend uses GraphQL, create a service to send queries:

```csharp
// Services/GraphqlService.cs
using System.Net.Http.Json;
using System.Text.Json;

namespace CSC13001_my_shop_project.Services;

public class GraphqlService
{
    private readonly HttpClient _client;

    public GraphqlService(IHttpClientFactory httpClientFactory)
    {
        _client = httpClientFactory.CreateClient("BackendApi");
    }

    public async Task<JsonElement> QueryAsync(string query, object? variables = null)
    {
        var payload = new { query, variables };

        var response = await _client.PostAsJsonAsync("/graphql", payload);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        return result.GetProperty("data");
    }
}
```

### Step 4 — Register Services in DI

Add the service to `App.xaml.cs` → `.ConfigureServices()`:

```csharp
.ConfigureServices((context, services) =>
{
    // Existing registrations
    services.AddSingleton<NavigationStateStore>();
    services.AddTransient<ProductDetailViewModel>();
    services.AddSingleton<AppStateService>();

    // API services
    services.AddSingleton<GraphqlService>();
    // services.AddTransient<IProductService, ProductService>();
    // services.AddTransient<IOrderService, OrderService>();
})
```

### Step 5 — Use in ViewModels

Inject the service into your ViewModels via constructor injection:

```csharp
// Example: Presentation/Products/ProductsViewModel.cs
public partial class ProductsViewModel : ObservableObject
{
    private readonly GraphqlService _api;

    public ProductsViewModel(GraphqlService api)
    {
        _api = api;
        _ = LoadProductsAsync();
    }

    private async Task LoadProductsAsync()
    {
        var data = await _api.QueryAsync(@"
            query {
                products {
                    id
                    name
                    price
                    stock
                    category
                    imageUrl
                }
            }
        ");

        // Parse and populate product list from 'data'
    }
}
```

---

## Architecture Diagram

```
┌─────────────────────────────────────────────────────────────┐
│  FRONTEND (Uno Platform / WinUI 3)                          │
│                                                             │
│  appsettings.development.json                               │
│    └── ApiClient:Url = "http://localhost:4000"               │
│              │                                              │
│              ▼                                              │
│  App.xaml.cs  →  .UseHttp()  →  HttpClientFactory           │
│                                      │                      │
│                                      ▼                      │
│                              GraphqlService                 │
│                              (POST /graphql)                │
│                                      │                      │
│              ┌───────────────────────┼────────────────┐     │
│              ▼                       ▼                ▼     │
│       ProductService          OrderService      AuthService │
│              │                       │                │     │
│              ▼                       ▼                ▼     │
│       ProductsViewModel       OrderViewModel    LoginVM     │
│              │                       │                │     │
│              ▼                       ▼                ▼     │
│       ProductsPage            OrderListPage     LoginPage   │
└─────────────────────────────────────────────────────────────┘
                               │
                               │  HTTP POST
                               ▼
┌─────────────────────────────────────────────────────────────┐
│  BACKEND (Node.js + Express + Apollo)                       │
│                                                             │
│  .env  →  PORT=4000, DB_HOST, JWT_SECRET, etc.              │
│                                                             │
│  http://localhost:4000/graphql                               │
│       │                                                     │
│       ├── Query: products, orders, dashboard stats          │
│       ├── Mutation: login, createOrder, updateProduct       │
│       └── Middleware: JWT auth, error handling              │
│                          │                                  │
│              ┌───────────┼───────────┐                      │
│              ▼           ▼           ▼                      │
│         PostgreSQL   Elasticsearch  Redis                   │
│         (port 5432)  (port 9200)   (port 6379)              │
└─────────────────────────────────────────────────────────────┘
```

---

## Server Configuration Page (Runtime Override)

The frontend also includes a **ServerConfigurationPage** that allows users to change connection settings at runtime. These values are stored in `ApplicationData.LocalSettings` (Windows local storage) and persist across app restarts.

| Setting     | Default Value      | LocalSettings Key         |
| :---------- | :----------------- | :------------------------ |
| Server URL  | `http://localhost`  | `ServerConfig.ServerUrl`  |
| Port        | `5432`              | `ServerConfig.Port`       |
| Database    | `luminahaven_db`    | `ServerConfig.Database`   |
| Username    | `admin`             | `ServerConfig.Username`   |
| Password    | _(empty)_           | `ServerConfig.Password`   |
| Enable SSL  | `true`              | `ServerConfig.EnableSsl`  |

> [!TIP]
> Use `appsettings.json` for the **default API URL** (build-time config).
> Use the **ServerConfigurationPage** for runtime overrides (e.g., switching servers).

---

## Quick Reference: Where Config Lives

| What                    | Frontend (Uno/WinUI)                  | Backend (Node.js)        |
| :---------------------- | :------------------------------------ | :----------------------- |
| Config file             | `appsettings.json`                    | `.env`                   |
| Config template         | Embedded in project                   | `.env.example`           |
| API URL                 | `ApiClient:Url` in appsettings        | `PORT` in `.env`         |
| Secrets (JWT, DB pass)  | Not needed on frontend                | In `.env`                |
| Gitignored?             | ❌ appsettings is committed            | ✅ `.env` is gitignored  |
| Runtime override        | ServerConfigurationPage + LocalSettings | N/A                    |
