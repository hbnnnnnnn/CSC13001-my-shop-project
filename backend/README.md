# MyShop Backend

Node.js + Express + Apollo GraphQL + PostgreSQL + Elasticsearch

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
cd backend
npm install (if run docker-compose up --build before npm install and get error, run npm install and docker compose down before run docker-compose up --build)
docker-compose up --build
```

This starts three services:

- **backend** — Node.js server with hot-reload (nodemon) at `http://localhost:4000/graphql`
- **db** — PostgreSQL 15 database
- **elasticsearch** — Elasticsearch at `http://localhost:9200`

On the first run, the database schema and seed data are created automatically. Elasticsearch index is also created and synced from the database on first run — subsequent restarts skip the sync if the index already exists.

### 3. Verify

- GraphQL Playground: http://localhost:4000/graphql
- Health check: http://localhost:4000/health

## pgAdmin (Optional Database UI)

Start pgAdmin and Kibana alongside the other services:

```bash
docker-compose --profile tools up -d
```

Open http://localhost:5050 and log in with the credentials from your `.env` file:

- **Email**: `admin@myshop.dev`
- **Password**: `admin`

### Connect to the database in pgAdmin

Click **Add New Server**, then fill in:

| Field                          | Value      |
| ------------------------------ | ---------- |
| **General → Name**             | `myshop`   |
| **Connection → Host**          | `db`       |
| **Connection → Port**          | `5432`     |
| **Connection → Database**      | `myshop`   |
| **Connection → Username**      | `postgres` |
| **Connection → Password**      | `postgres` |
| **Connection → Save password** | Yes        |

Tables are at: Servers → myshop → Databases → myshop → Schemas → public → Tables

### Stop pgAdmin (keep backend running)

```bash
docker-compose --profile tools stop pgadmin
```

## Kibana (Optional Elasticsearch UI)

Kibana runs as part of the `tools` profile at `http://localhost:5601`.

Useful things to do in Kibana:

- **Stack Management → Index Management** — view the `products` index, doc count, health, and mappings
- **Analytics → Discover** — browse and filter indexed documents interactively
- **Management → Dev Tools** — run raw Elasticsearch API queries

## Common Commands

| Command                                       | Description                                           |
| --------------------------------------------- | ----------------------------------------------------- |
| `docker-compose up -d`                        | Start backend + database + elasticsearch              |
| `docker-compose up -d --build`                | Rebuild and start (after Dockerfile changes)          |
| `docker-compose --profile tools up -d`        | Start everything including pgAdmin and Kibana         |
| `docker-compose exec backend npm run db:seed` | Reset and re-seed sample data                         |
| `docker-compose down`                         | Stop all containers                                   |
| `docker-compose down -v`                      | Stop all and **delete all volumes** (full reset)      |
| `docker volume rm backend_esdata`             | Delete only the Elasticsearch volume (forces re-sync) |
| `docker-compose logs -f backend`              | Stream backend logs                                   |
| `docker-compose logs -f db`                   | Stream database logs                                  |
| `docker-compose logs -f elasticsearch`        | Stream Elasticsearch logs                             |

## Project Structure

```
backend/
├── database/
│   ├── migrations/          # SQL schema (auto-runs on first DB creation)
│   └── seeds/               # Sample data (auto-runs on first DB creation)
├── src/
│   ├── config/
│   │   ├── db.js            # PostgreSQL connection pool
│   │   └── elasticsearch.js # Elasticsearch client
│   ├── graphql/
│   │   ├── schema/          # GraphQL type definitions
│   │   ├── resolvers/       # GraphQL resolvers
│   │   ├── loaders/         # GraphQL loaders
│   │   └── index.js         # Auto-merges all schemas and resolvers
│   ├── scripts/
│   │   ├── initDb.js        # Manual seed script (npm run db:seed)
│   │   └── syncElastic.js   # ES sync script (runs on container start)
│   ├── services/
│   │   ├── product.service.js  # Product business logic + ES sync on CRUD
│   │   └── search.service.js   # Elasticsearch index and search operations
│   ├── utils/
│   │   ├── image.js          # Image upload utilities
│   │   └── cache.utils.js    # Cache utilities
│   ├── repositories/
│   │   ├── base.repository.js  # Base repository for all repositories
│   │   ├── product.repository.js # Product repository
│   │   └── search.repository.js  # Search repository
│   ├── middlewares/
│   │   ├── auth.middleware.js  # Authentication middleware
│   │   └── upload.middleware.js  # Upload middleware
│   ├── routes/
│   │   └── upload.routes.js  # Upload routes
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
- Database data persists in a Docker volume (`pgdata` and `esdata`). To fully reset, run `docker-compose down -v`.
- The backend container mounts the source code, so file changes trigger auto-reload via nodemon.

