Documentation: Deploying the CS2 Price Prediction Project

 Project Description

The CS2 Price Prediction API is a backend service built with ASP.NET Core that:
Provides metadata (weapons, skins, patterns, stickers)
Calculates sticker features
Sends the collected data to a Python ML service and returns the predicted price

Tech Stack:
ASP.NET Core 8
EF Core + PostgreSQL
Python FastAPI (ML service)
Docker + Docker Compose
Swagger UI

Requirements

For local launch:
.NET SDK 8.0
PostgreSQL 15+
Python 3.10+ (for ML)
pip / poetry / venv
Git

For running in containers:
Docker 20+
Docker Compose 2.0+

Running the Project via Docker (Recommended)
This guide explains how to run the project using Docker, even if it's your first time running a .NET application.
No need to install .NET SDK, PostgreSQL, or Python — Docker will set up everything automatically.

1. Navigate to the Project Folder
Open PowerShell or Command Prompt, then go to the folder where the project is located:
cd "C:\Path\To\Project\cs2price_prediction"

2. Build the Docker Containers
 Run:
 docker compose build
 If the command doesn't work, try:
 docker-compose build

3. Start the Project
Run:
docker compose up
Or run in the background:
docker compose up -d

4. Verify That Everything Works
http://localhost:8087/swagger
http://localhost:8000/docs#/

5.Stopping the Containers
docker compose down
