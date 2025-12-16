# cs2price_prediction — Testing & Quality Evaluation

This repository contains the machine-learning–based CS2 skin price prediction service and all testing documentation prepared as part of the diploma project.

The goal of the system is to:
- Predict CS2 skin market prices using ML models.
- Provide explanation endpoints with model-level reasoning (Explain & Explain-V2).
- Supply metadata for skins, weapons, stickers, and patterns.

## Project Structure

```
cs2price_prediction/
├──dock/test
│         ├── test_plan.md
│         ├── test_cases_en.md
│         ├── test_cases_ru.md
│         ├── requirements.md
│         ├── test_report.md
│         ├── quantitative_metrics.md
│         └── img/
│              ├── pass_rate.png
│              ├── avg_response_time.png
└── tools_postman_testing/
    ├── postman_explain.json
    ├── postman_predict.json
    └── postman_meta.json
```

## Running the API

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
## Running Postman Tests

1. Import collection:
   `tools_postman_testing/postman_explain`

2. Open **Collection Runner** → Run tests.

## Available Metrics

ML metrics in `docs/quantitative_metrics.md`  
API performance charts in `docs/img/`

## Documentation

All testing artifacts are in the `dock/test` folder:
- Test Plan
- Requirements 
- Test cases
- Test report
- Quantitative Metrics
- Final Summary

## Summary

This project includes full functional, qualitative, and quantitative testing of the CS2 price prediction API.
