# Containerization Documentation (Short Version)

## 1. Overview

The application is fully containerized using **Docker** and orchestrated with **Docker Compose**.  
The system consists of four main containers:

1. API (ASP.NET Core)
2. ML Service (FastAPI, CatBoost models)
3. PostgreSQL Database
4. API Dev Container (development only)

This setup ensures reproducible builds, isolated environments, and easy deployment.

---

## 2. Containers Summary

### 2.1 API Container (`cs2-api`)

**Purpose:**  
Handles HTTP requests, retrieves metadata from PostgreSQL, constructs feature vectors, sends them to the ML service, and returns predictions.

**Base Image:**  
mcr.microsoft.com/dotnet/aspnet:8.0

**Ports:**  
Internal: 8080  
External: 8087

**Environment Variables:**  
- ASPNETCORE_ENVIRONMENT  
- Connection strings  
- ML_SERVICE_URL  
- ADMIN_AUTH_APIKEY  

**Resource Usage:**  
- CPU: 0–1% idle, 5–10% under load  
- RAM: ~120–180 MB  

---

### 2.2 ML Container (`cs2-ml`)

**Purpose:**  
Loads 6 CatBoost models and performs ML predictions.

**Base Image:**  
python:3.11-slim

**Port:**  
8000

**Resource Usage:**  
- CPU: 1–3% idle, 15–40% under load  
- RAM: 300–520 MB  

---

### 2.3 PostgreSQL Container (`cs2-postgres`)

**Purpose:**  
Stores weapon metadata, skins, wear tiers, stickers, patterns, and all ML reference data.

**Base Image:**  
postgres:16-alpine

**Volumes:**  
- db-data (persistent storage)  
- init.sql (schema initialization)  

**Resource Usage:**  
- CPU: <1% idle, up to 10% under load  
- RAM: 50–120 MB  

---

### 2.4 API Dev Container (`cs2-api-dev`)

**Purpose:**  
Used only during development for hot reload and debugging.

**Base Image:**  
mcr.microsoft.com/dotnet/sdk:8.0

**Resource Usage:**  
- CPU: 3–8%  
- RAM: 300–500 MB  

---

## 3. System Requirements

### Minimum RAM Required: **4 GB**

Rationale:
- API: ~180 MB  
- ML service: up to 520 MB  
- PostgreSQL: 120 MB  
- Docker overhead: ~300 MB  
- OS: 1.5–2 GB  

Total ≈ 2.3–3.0 GB → **4 GB is the safe minimum**.

### Recommended: **8 GB**

---

### CPU Requirements

Based on testing on a 4-core machine:

- Idle CPU: 5–8%
- Under load: 25–45% (mainly ML inference)

**Minimum:** 2 cores  
**Optimal:** 4 cores  

---

## 4. Startup Times

- PostgreSQL: 4–6 seconds  
- ML Service: 2–4 seconds  
- API: 4–6 seconds  
- **Full system ready:** ~12–15 seconds after `docker compose up`

---

## 5. Summary

The containerization setup:

- Ensures reproducible builds  
- Provides isolated services  
- Supports production & development  
- Meets performance requirements  
- Runs within 4 GB RAM and moderate CPU load  

