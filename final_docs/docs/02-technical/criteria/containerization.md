## Containerization Technical Documentation (Short Version)

### Docker Compose

The project is fully containerized using **Docker Compose**, which orchestrates all required services.

#### docker-compose.yml

Defines the following services:

- **cs2-api** – ASP.NET Core production API  
- **cs2-api-dev** – ASP.NET Core API with hot reload (dev only)  
- **cs2-ml** – FastAPI ML service with CatBoost models  
- **cs2-postgres** – PostgreSQL database  
- **Volumes** – Persistent DB storage  

All services are isolated, configured via environment variables, and connected through a shared Docker network.


### Build & Interaction Diagrams

#### Build Pipeline

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

#### Runtime Interaction Pipeline

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

#### Request Flow

1. Client sends request to API.
2. API reads metadata from PostgreSQL.
3. API sends feature JSON to ML service.
4. ML service returns predicted price.
5. API responds to client.


### Docker Images Overview

#### API Image — cs2price_prediction-api

- ASP.NET Core 8 Web API.
- Handles requests, DB access, ML communication.
- Multi-stage build:
  - Build: `dotnet/sdk:8.0`
  - Runtime: `dotnet/aspnet:8.0`
- Final size: **~335 MB**
- Optimized with multi-stage build, `.dockerignore`, env secrets.
- Exposes port **8080**.

#### ML Image — cs2price_prediction-ml-service

- FastAPI service with **6 CatBoost models**.
- Image: `python:3.11-slim`
- Final size: **~1.3 GB** (ML dependencies).
- Optimized pip/apt usage, non-root user.
- Exposes port **8000**.

#### Database Image — postgres:16-alpine

- Stores skins, patterns, stickers, and metadata.
- Uses persistent volume `db-data`.
- Final size: **~395 MB**.
- Credentials via `.env`.


### Development Container

#### cs2-api-dev

- Development-only.
- `dotnet watch` hot reload.
- Image: `dotnet/sdk:8.0`
- Size: **~1.19 GB**.


### Containers & Ports

| Container | Role | Port |
|---------|-----|------|
| cs2-api | Main API | 8087 → 8080 |
| cs2-ml | ML inference | 8000 |
| cs2-postgres | Database | internal |

**API healthcheck:** `/health` endpoint.


### Performance Summary

- Total RAM usage: **600–800 MB**
- Cold start: **12–15 s**
- Prediction latency: **< 50 ms**


### Conclusion

The Docker-based architecture is modular, efficient, and suitable for both production and development, fully meeting diploma technical requirements.
