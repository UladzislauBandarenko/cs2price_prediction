
# Containerization & Deployment Documentation

## 1. Docker Compose

The project is fully containerized using **Docker Compose**, which orchestrates all services required for the system to operate.

### 1.1 docker-compose.yml

The `docker-compose.yml` file (see `dev` branch in the repository) defines the following services:

- **cs2-api** – ASP.NET Core production API
- **cs2-api-dev** – ASP.NET Core development API (hot reload)
- **cs2-ml** – FastAPI-based ML service with CatBoost models
- **cs2-postgres** – PostgreSQL database
- **Volumes** – Persistent storage for database data

Each service is isolated, configured via environment variables, and connected through a shared Docker network.

---

## 2. Build & Interaction Diagrams

### 2.1 Build Pipeline

```
docker compose build
        │
        ├── Build API image
        │   ├── dotnet restore
        │   ├── dotnet publish
        │   └── copy published output
        │
        ├── Pull PostgreSQL image
        │   └── postgres:16-alpine
        │
        └── Build ML image
            ├── install system deps
            ├── pip install requirements
            └── copy FastAPI app + models
```

### 2.2 Runtime Interaction Pipeline

```
User / Client
      │
      ▼
ASP.NET Core API (cs2-api)
      │
      ├── PostgreSQL (cs2-postgres)
      │     └── reference & metadata queries
      │
      └── ML Service (cs2-ml)
            └── price prediction
```

### 2.3 Request Flow

1. User sends request → API (filters, skin selection).
2. API reads reference data from DB (skins, patterns, stickers).
3. API builds feature JSON and sends it to ML service.
4. ML service returns predicted price.
5. API responds to the user with final price.

---

## 3. Docker Images Description

## 3.1 API Image — cs2price_prediction-api

### Purpose & Functionality

The API image hosts the ASP.NET Core 8 Web API and is responsible for:

- Handling client HTTP requests.
- Fetching metadata from PostgreSQL.
- Building ML feature vectors.
- Communicating with the ML microservice.
- Returning final predicted prices.
- Running the backend in a production-ready environment.

### Base Image & Rationale

**Multi-stage Dockerfile:**

1. **Build stage**
   - Image: `mcr.microsoft.com/dotnet/sdk:8.0`
   - Used for restore, build, and publish.

2. **Runtime stage**
   - Image: `mcr.microsoft.com/dotnet/aspnet:8.0`
   - Lightweight, secure, production-optimized runtime.

### Final Image Size

- **334.84 MB**
- Fully satisfies the requirement: **Backend < 1 GB**.

### Optimizations Applied

- Multi-stage build (SDK excluded from runtime).
- `.dockerignore` excludes build artifacts and VCS files.
- Only published output copied.
- Secrets via environment variables.
- Runs as non-root user.
- Exposes only port **8080**.

---

## 3.2 ML Image — cs2price_prediction-ml-service

### Purpose & Functionality

This image runs the FastAPI-based ML service and:

- Loads **6 CatBoost models** at startup.
- Exposes `/predict/*` endpoints.
- Processes numerical and categorical features.
- Returns predicted prices.

### Base Image & Justification

- Image: `python:3.11-slim`
- Reasons:
  - Small OS footprint.
  - Compatibility with CatBoost and NumPy.
  - Official, security-patched Python image.

### Final Image Size

- **1.30 GB**
- Larger size justified by ML dependencies and models.

### Optimizations Applied

- `pip install --no-cache-dir`
- `apt-get --no-install-recommends`
- Cleanup of APT cache.
- `.dockerignore` excludes caches and unused artifacts.
- Runs as non-root user.
- Exposes only port **8000**.

---

## 3.3 Database Image — postgres:16-alpine

### Purpose & Functionality

Stores all persistent data:

- Skins, weapons, wear tiers.
- Patterns (CH, Fade, Doppler).
- Stickers and sticker prices.
- Metadata for ML feature construction.

### Base Image & Justification

- Image: `postgres:16-alpine`
- Benefits:
  - Smaller footprint.
  - Faster startup.
  - Lower resource usage.

### Final Image Size

- **394.74 MB**
- Uses persistent volume `db-data`.

### Optimizations Applied

- Official Postgres image.
- Read-only schema initialization script.
- Stateless container with external volume.
- Credentials via `.env` file.

---

## 3.4 API Dev Image — cs2-api-dev

**Development-only container.**

### Purpose

- Hot reload via `dotnet watch`.
- Debugging and testing.
- No production usage.

### Base Image

- `mcr.microsoft.com/dotnet/sdk:8.0`

### Final Image Size

- **1.19 GB**
- Acceptable for development use only.

---

## 4. Containers Description

### 4.1 API Container — cs2-api

**Role:** Main entry point for clients.

| Type | Port |
|-----|------|
| Internal | 8080 |
| External | 8087 |

**Healthcheck:**  
HTTP `/health` endpoint.

---

### 4.2 ML Service Container — cs2-ml

**Role:** Performs ML inference.

- Port: **8000**
- Models loaded into memory at startup.
- No volumes required.

---

### 4.3 PostgreSQL Container — cs2-postgres

**Role:** Persistent data storage.

**Volumes:**

| Volume | Purpose |
|------|--------|
| db-data | PostgreSQL data |
| init.sql | Schema initialization |

---

## 5. Metrics & Performance

### 5.1 Image Build Time

| Service | Time |
|-------|------|
| API | 45–55s |
| ML | 60–80s |
| DB | Instant |
| API Dev | 10–15s |

### 5.2 Runtime Resource Usage

| Container | RAM | CPU |
|---------|-----|-----|
| API | 120–180 MB | up to 10% |
| ML | 300–520 MB | up to 40% |
| DB | 50–120 MB | up to 10% |

### 5.3 System Summary

- Total RAM: **600–800 MB**
- Cold start: **12–15 seconds**
- Predictions < **50 ms**

---

## 6. Conclusion

The containerized architecture:

- Is modular and scalable.
- Meets all performance and size constraints.
- Supports both production and development workflows.
- Fully complies with diploma technical requirements.
