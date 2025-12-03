#  CS2 Price Prediction – Deployment Documentation

## Project Description

The **CS2 Price Prediction API** is a backend service built with **ASP.NET Core** that:

- Provides metadata:
  - weapons  
  - skins  
  - patterns  
  - stickers  
- Calculates sticker-feature metrics  
- Sends the processed data to a Python ML service  
- Returns the predicted market price

###  Tech Stack

| Component | Technology |
|----------|------------|
| Backend | ASP.NET Core 8 |
| ORM | EF Core + PostgreSQL |
| ML Service | Python FastAPI |
| Containerization | Docker + Docker Compose |
| API Docs | Swagger UI |

---

## 📦 Requirements

### Local Development Requirements

| Requirement | Version |
|------------|---------|
| .NET SDK | 8.0 |
| PostgreSQL | 15+ |
| Python | 3.10+ |
| Python Tools | pip / poetry / venv |
| Git | any |

### Docker Requirements

| Requirement | Version |
|------------|---------|
| Docker | 20+ |
| Docker Compose | 2.0+ |

---

#  Running the Project via Docker (Recommended)

Using Docker allows you to run the project **without installing .NET, PostgreSQL, or Python** —  
everything is set up automatically inside containers.

---

##  1. Navigate to the Project Folder

Open **PowerShell** or **Command Prompt** and go to the project directory:

```sh
cd "C:\Path\To\Project\cs2price_prediction"


## 2. Build the Docker Containers
Run:
```sh
docker compose build
If the command doesn't work, try:
```sh
docker-compose build

##  3. Start the Project
Run:
```sh
docker compose up
Or run in the background:
```sh
docker compose up -d

## 4. Verify That Everything Works
http://localhost:8087/swagger
http://localhost:8000/docs#/

## 5.Stopping the Containers
```sh
docker compose down
