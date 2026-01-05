# 2. Technical Implementation

This section covers the technical architecture, design decisions, and implementation details of the CS2 Skin Price Analysis and Prediction System.

## Contents

- [Tech Stack](tech-stack.md)
- [Criteria Documentation](criteria/) – Architecture Decision Records (ADR)
- [Deployment](deployment.md)

## Solution Architecture

### High-Level Architecture

```
┌──────────────────────────────────────────────────────────────────────────┐
│              CS2 Skin Price Analysis & Prediction System                 │
├──────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│   ┌─────────────┐       ┌──────────────────┐       ┌──────────────────┐  │ 
│   │   Client    │ ────▶ │   Backend API    │ ────▶ │    PostgreSQL    │  │
│   │  (Web UI)   │ ◀──── │ (ASP.NET Core)   │ ◀──── │     Database     │  │
│   └─────────────┘       └──────────────────┘       └──────────────────┘  │
│         │                        │                                       │
│         │                        ▼                                       │
│         │              ┌──────────────────┐                              │
│         │              │   ML Service     │                              │
│         │              │ (Dockerized)     │                              │
│         │              └──────────────────┘                              │
│         │                        │                                       │
│         ▼                        ▼                                       │
│   ┌──────────────────────────────────────────────┐                       │
│   │        External Market Services (APIs)       │                       │
│   └──────────────────────────────────────────────┘                       │
│                                                                          │
└──────────────────────────────────────────────────────────────────────────┘
```

Detailed architecture diagrams are provided in `02-technical/diagrams/`.

### System Components

| Component | Description | Technology |
|-----------|-------------|------------|
| **Frontend** | Web-based user interface for data input and visualization | Razor Pages |
| **Backend** | Business logic, validation, orchestration | ASP.NET Core Web API |
| **Database** | Persistent data storage | PostgreSQL |
| **ML Service** | Price prediction and explainability | Python, Docker, XGBoost, SHAP |
| **External Services** | Market data providers | Steam, Buff, Skinport |

### Data Flow

```
[User Action] → [Frontend] → [API Request] → [Backend]
                                                 │
                                                 ▼
                                          [Business Logic]
                                                 │
                                                 ▼
                                          [ML Service]
                                                 │
                                                 ▼
                                          [Data Layer]
                                                 │
                                                 ▼
                                          [Database]
                                                 │
                                                 ▼
                                          [Response]
                                                 │
[UI Update] ← [Frontend] ← [API Response] ←─────┘
```

## Key Technical Decisions

| Decision | Rationale | Alternatives Considered |
|----------|-----------|------------------------|
| ASP.NET Core Web API | Performance, maintainability | Node.js, Spring Boot |
| PostgreSQL | Relational consistency | MongoDB |
| Separate ML Service | Scalability, isolation | Embedded ML |
| SHAP Analysis | Prediction explainability | Black-box models |

## Security Overview

| Aspect | Implementation |
|--------|----------------|
| **Authentication** | JWT-based authentication |
| **Authorization** | Role-Based Access Control (RBAC) |
| **Data Protection** | HTTPS encryption |
| **Input Validation** | Server-side validation |
| **Secrets Management** | Environment variables |
