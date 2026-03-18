# MyShop Backend

Node.js + Express + Apollo GraphQL + PostgreSQL

## Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) installed and running

## Quick Start

### 1. Create environment file

Copy the example and adjust if needed:

```bash
cp .env.example .env
```

Default values work out of the box for local development.

### 2. Start the backend

```bash
docker-compose up -d
```

This starts two services:
- **backend** — Node.js server with hot-reload (nodemon) at `http://localhost:4000/graphql`
- **db** — PostgreSQL 15 database

On the first run, the database schema and seed data are created automatically.

### 3. Verify

- GraphQL Playground: http://localhost:4000/graphql
- Health check: http://localhost:4000/health

## pgAdmin (Optional Database UI)

Start pgAdmin alongside the other services:

```bash
docker-compose --profile tools up -d
```

Open http://localhost:5050 and log in with the credentials from your `.env` file:
- **Email**: `admin@myshop.dev`
- **Password**: `admin`

### Connect to the database in pgAdmin

Click **Add New Server**, then fill in:

| Field | Value |
|---|---|
| **General → Name** | `myshop` |
| **Connection → Host** | `db` |
| **Connection → Port** | `5432` |
| **Connection → Database** | `myshop` |
| **Connection → Username** | `postgres` |
| **Connection → Password** | `postgres` |
| **Connection → Save password** | Yes |

Tables are at: Servers → myshop → Databases → myshop → Schemas → public → Tables

### Stop pgAdmin (keep backend running)

```bash
docker-compose --profile tools stop pgadmin
```

## Common Commands

| Command | Description |
|---|---|
| `docker-compose up -d` | Start backend + database |
| `docker-compose up -d --build` | Rebuild and start (after Dockerfile changes) |
| `docker-compose --profile tools up -d` | Start everything including pgAdmin |
| `docker-compose exec backend npm run db:seed` | Reset and re-seed sample data |
| `docker-compose down` | Stop all containers |
| `docker-compose down -v` | Stop all and **delete database volume** (full reset) |
| `docker-compose logs -f backend` | Stream backend logs |
| `docker-compose logs -f db` | Stream database logs |

## Project Structure

```
backend/
├── database/
│   ├── migrations/          # SQL schema (auto-runs on first DB creation)
│   └── seeds/               # Sample data (auto-runs on first DB creation)
├── src/
│   ├── config/db.js         # PostgreSQL connection pool
│   ├── graphql/
│   │   ├── schema/          # GraphQL type definitions
│   │   ├── resolvers/       # GraphQL resolvers
│   │   └── index.js         # Auto-merges all schemas and resolvers
│   ├── scripts/initDb.js    # Manual seed script (npm run db:seed)
│   ├── app.js               # Express app setup and REST routes
│   └── server.js            # Entry point: Apollo Server bootstrap
├── .env.example             # Environment template (commit this)
├── .env                     # Local environment (DO NOT commit)
├── docker-compose.yml
├── Dockerfile
└── package.json
```

## Notes

- `.env` is gitignored — each team member copies `.env.example` and adjusts if needed.
- Database data persists in a Docker volume (`pgdata`). To fully reset, run `docker-compose down -v`.
- The backend container mounts the source code, so file changes trigger auto-reload via nodemon.
