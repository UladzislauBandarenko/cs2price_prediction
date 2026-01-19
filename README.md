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

##  Requirements

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

# Running the Project via Docker (Recommended)

Using Docker allows you to run the project without installing .NET, PostgreSQL, or Python — everything is set up automatically inside containers.

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

Development (hot reload):

```sh
docker compose --profile dev up
```

Production-like run:

```sh
docker compose --profile prod up
```

## 4. Verify That Everything Works

Open:

http://localhost:8087/swagger  
http://localhost:8000/docs#/

## 5. Stopping the Containers

```sh
docker compose down
```

---

#  Running the Frontend

The frontend is a simple HTML/CSS/JS application located in the `frontend` folder.

## Quick Start

1. Make sure the backend is running at `http://localhost:8087`

2. Navigate to the frontend folder:

```sh
cd "c:\Users\bonda\source\repos\cs2price_prediction\frontend"
```

3. Start a simple HTTP server:

```sh
python -m http.server 8080
```

4. Open your browser and navigate to:

```
http://localhost:8080
```

## Option 2: Using Live Server (VS Code Extension)

1. Install the "Live Server" extension in VS Code
2. Right-click on `frontend/index.html`
3. Select "Open with Live Server"

## Option 3: Using Node.js http-server

1. Install http-server globally:

```sh
npm install -g http-server
```

2. Navigate to the frontend folder and run:

```sh
cd "C:\Path\To\Project\cs2price_prediction\frontend"
http-server -p 8080
```

3. Open your browser and navigate to:

```
http://localhost:8080
```

---

#  Running the Frontend - Step by Step

1. **Select Weapon Type** (Rifle, Pistol, SMG, etc.)
2. **Select Weapon** (AK-47, M4A4, etc.)
3. **Select Skin** (Redline, Asiimov, etc.)
4. **Select Wear Tier** (Factory New, Minimal Wear, etc.)
5. **Select Pattern** (Optional - Doppler Phase, Fade %, etc.)
6. **Add Stickers** (Optional - search and select up to 4 stickers)
7. **Enter Float Value** (0.0 - 1.0, lower = better condition)
8. **Check StatTrak™** (if applicable)
9. **Click "Get Price Prediction"** - displays predicted price
10. **Click "Get AI Explanation"** (optional) - shows basic AI analysis
11. **Click "Get Detailed Explanation"** (optional, appears only after step 10) - shows comprehensive AI analysis

The prediction will show:
- **Predicted market price**
- **Sticker features** (if stickers are selected):
  - Total stickers count
  - Total stickers value (from backend calculation)
  - Average sticker value (from backend calculation)
  - Maximum sticker value (from backend calculation)

## AI Explanation Features

### Basic AI Explanation (Step 10)
- Uses **gpt-4o-mini** (fast and cost-effective)
- Provides quick analysis of the price prediction
- Explains key factors affecting the price
- **Required before accessing detailed explanation**

### Detailed AI Explanation (Step 11)
- **Only available after getting basic explanation**
- Uses **gpt-4.1-mini** (more advanced model)
- Provides comprehensive market analysis
- Detailed breakdown of all pricing factors
- Investment recommendations

Both explanations use the same prediction data including:
- Predicted price
- Skin, wear tier, float value
- Pattern and stickers information

## Important: CORS Setup

If you get CORS errors, make sure `Program.cs` has CORS enabled:

```csharp
// Add this AFTER builder.Services.AddControllers()
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:8080", "http://127.0.0.1:8080")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add this BEFORE app.MapControllers()
app.UseCors("AllowFrontend");
```

Then rebuild Docker:
```sh
docker compose down
docker compose build --no-cache
docker compose --profile prod up
```

---
