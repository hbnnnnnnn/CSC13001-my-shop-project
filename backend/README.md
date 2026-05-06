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
| `docker-compose exec backend npm run db:migrate` | Run new SQL migrations and seeds (idempotent)  |
| `docker-compose exec backend npm run es:sync`    | Re-sync database data to Elasticsearch index   |
| `docker-compose exec redis redis-cli FLUSHALL`   | Clear all Redis cache (useful after migrations) |
| `docker-compose down`                         | Stop all containers                                   |
| `docker-compose down -v`                      | Stop all and **delete all volumes** (full reset)      |
| `docker volume rm backend_esdata`             | Delete only the Elasticsearch volume (forces re-sync) |
| `docker-compose logs -f backend`              | Stream backend logs                                   |
| `docker-compose logs -f db`                   | Stream database logs                                  |
| `docker-compose logs -f elasticsearch`        | Stream Elasticsearch logs                             |
| `POST /api/config/db`                        | REST API để cập nhật cấu hình kết nối DB từ Frontend  |

## Dynamic Database Configuration

Hệ thống cho phép thay đổi thông tin kết nối CSDL PostgreSQL động thông qua REST API (để phục vụ yêu cầu cấu hình từ giao diện Desktop).

- **Cấu hình ưu tiên:** Hệ thống ưu tiên đọc file `src/config/db.config.json`. Nếu không có, sẽ sử dụng biến môi trường trong `.env`.
- **API Endpoint:** `POST http://localhost:4000/api/config/db`
- **Tác vụ:** Khi gọi API này, Backend sẽ kiểm tra kết nối mới. Nếu thành công, nó sẽ lưu vào file JSON, ngắt kết nối cũ và tự động chuyển sang sử dụng kết nối mới mà không cần restart server.

## Project Structure

```
backend/
├── database/
│   ├── migrations/          # SQL structural changes (01_init, 02_indexes, etc.)
│   └── seeds/               # Initial/dummy data (01_dummy_data.sql)
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
│   │   ├── initDb.js        # Migration & seed runner (npm run db:migrate)
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
├── package.json
```

## Notes

- `.env` is gitignored — each team member copies `.env.example` and adjusts if needed.
- Database data persists in a Docker volume (`pgdata` and `esdata`). To fully reset, run `docker-compose down -v`.
- The backend container mounts the source code, so file changes trigger auto-reload via nodemon.

## Database Migrations

This project uses a simple, custom migration system located in `src/scripts/initDb.js`.

- **How it works**: It scans `database/migrations` and `database/seeds`, runs any `.sql` file that hasn't been executed yet, and records the execution in a `schema_migrations` table in the database.
- **Execution**:
    - **New setup**: Docker automatically runs the files in `/docker-entrypoint-initdb.d/` on first startup.
    - **Existing setup**: Khi bạn pull code mới có các file `.sql` mới, hãy chạy:
      ```bash
      docker-compose exec backend npm run db:migrate
      ```
    - **Sau khi migrate**: Nếu thay đổi liên quan đến dữ liệu Báo cáo hoặc Sản phẩm, hãy chạy:
      ```bash
      # 1. Đồng bộ Elasticsearch
      docker-compose exec backend npm run es:sync
      # 2. Xóa cache Redis để cập nhật số liệu mới
      docker-compose exec redis redis-cli FLUSHALL
      ```
- **Adding new changes**: Simply create a new `.sql` file in `database/migrations/` (e.g., `03_add_new_table.sql`). Use `IF NOT EXISTS` syntax where possible for safety.

