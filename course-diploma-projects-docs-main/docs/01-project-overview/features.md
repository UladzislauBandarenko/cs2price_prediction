# Features & Requirements

## Core Features (Epics)

| Epic ID | Feature | Description | Priority |
|------|--------|-------------|----------|
| E1 | Skin Data Input | Input of all CS2 skin parameters | Must |
| E2 | Price Prediction | ML-based skin price prediction | Must |
| E3 | Price Visualization | Historical price charts | Should |
| E4 | Factor Analysis | SHAP-based influence analysis | Should |
| E5 | Automated Updates | Market data synchronization | Should |
| E6 | API Documentation | Swagger documentation | Could |

## Functional Requirements – User Stories

### User Story 1 – Skin Data Input

**As a** CS2 player  
**I want** to enter detailed skin parameters  
**So that** I can receive an accurate price estimate.

**Acceptance Criteria:**
- All parameters are validated
- Data is stored in PostgreSQL
- StatTrak and non-StatTrak items are supported

---

### User Story 2 – Price Prediction

**As a** CS2 trader  
**I want** the system to predict skin prices  
**So that** I can make informed trading decisions.

**Acceptance Criteria:**
- Prediction response time ≤ 2 seconds
- Prediction is displayed in the UI
- Prediction history is persisted

---

### User Story 3 – Price Visualization

**As a** CS2 player or trader  
**I want** to see price trends and influencing factors  
**So that** I can analyze the market behavior.

**Acceptance Criteria:**
- Interactive charts are available
- Key influencing factors are visible

---

### User Story 4 – Automated Data Updates

**As an** system administrator  
**I want** the system to update market data automatically  
**So that** ML predictions remain accurate.

**Acceptance Criteria:**
- Data updates occur at least every 12 hours
- Errors are logged and monitored

---

### User Story 5 – API Documentation

**As a** developer  
**I want** comprehensive Swagger documentation  
**So that** system integration is simplified.

**Acceptance Criteria:**
- All API endpoints are documented
- Documentation is accessible via web interface

---

## Non-Functional Requirements

- Performance: response time ≤ 2 seconds
- Security: HTTPS, JWT, secure data storage
- Scalability: up to 500 concurrent users
- Availability: ≥ 99%
- Usability: responsive UI
- Deployment: Docker-based containerization
