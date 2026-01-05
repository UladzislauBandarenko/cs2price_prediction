# Deployment

## Deployment Model

The system is deployed using Docker containers to ensure consistent behavior across environments.

Components:
- Backend API container
- ML Service container
- PostgreSQL container

## 1. Navigate to the Project Folder

Open PowerShell or Command Prompt and go to the project directory:

```sh
cd "C:\Path\To\Project\cs2price_prediction"
```

## 2. Build the Docker Containers

Run:

```sh
docker compose build
```

If the command doesn't work, try:

```sh
docker-compose build
```

## 3. Start the Project

Run:

```sh
docker compose up
```

Or run in the background:

```sh
docker compose up -d
```

## 4. Verify That Everything Works

Open:

http://localhost:8087/swagger  
http://localhost:8000/docs#/

## 5. Stopping the Containers

```sh
docker compose down
```
## Monitoring

- Application logs via ASP.NET logging
- Health checks for service availability
